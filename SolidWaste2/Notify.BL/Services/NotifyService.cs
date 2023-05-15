using Common.Services.GraphApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Notify.DAL.Contexts;
using Notify.DM;
using System.ComponentModel.DataAnnotations;

namespace Notify.BL.Services;

public class NotifyService : INotifyService
{
    private readonly IDbContextFactory<NotifyDbContext> dbFactory;
    private readonly IGraphService graphService;
    private readonly NotifySettings notifySettings;

    public NotifyService(
        IDbContextFactory<NotifyDbContext> dbFactory,
        IGraphService graphService,
        IOptions<NotifySettings> options)
    {
        this.dbFactory = dbFactory;
        this.graphService = graphService;
        notifySettings = options.Value;
    }

    public async Task<ICollection<Notification>> GetByTo(string email)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Notifications
            .Where(n => n.To == email && !n.Deleted)
            .ToListAsync();
    }

    public async Task<Notification> GetById(int id, bool includeDeleted = false)
    {
        using var db = dbFactory.CreateDbContext();
        IQueryable<Notification> query = db.Notifications
            .Where(n => n.NotificationID == id);

        if (!includeDeleted)
            query = query.Where(n => !n.Deleted);

        return await query.FirstOrDefaultAsync();
    }

    public async Task Add(Notification notification)
    {
        _ = notification ?? throw new ArgumentNullException(nameof(notification));

        notification.AddDateTime ??= DateTime.Now;
        notification.Subject ??= "";
        notification.Body ??= "";

        Validator.ValidateObject(notification, new ValidationContext(notification));

        using var db = dbFactory.CreateDbContext();
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    public async Task ToggleReadById(int notificationId)
    {
        using var db = dbFactory.CreateDbContext();
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification == null)
            throw new ArgumentException("Notification not found");

        notification.ChgDateTime = DateTime.Now;
        notification.Read = !notification.Read;
        notification.Subject ??= "";
        notification.Body ??= "";

        Validator.ValidateObject(notification, new ValidationContext(notification));

        await db.SaveChangesAsync();
    }

    public async Task DeleteById(int notificationId)
    {
        using var db = dbFactory.CreateDbContext();
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification == null)
            return;

        notification.DelDateTime = DateTime.Now;
        notification.Deleted = true;

        Validator.ValidateObject(notification, new ValidationContext(notification));

        await db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountByTo(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return -1;

        using var db = dbFactory.CreateDbContext();
        return await db.Notifications
            .Where(n => n.To == email && !n.Read && !n.Deleted)
            .CountAsync();
    }

    public async Task<ICollection<Notification>> GetUnreadByTo(string email, int limit)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Array.Empty<Notification>();

        using var db = dbFactory.CreateDbContext();
        return await db.Notifications
            .Where(n => n.To == email && !n.Read && !n.Deleted)
            .OrderByDescending(n => n.NotificationID)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<ICollection<ApprovalUser>> GetApprovalUsers()
    {
        var rolesResult = await graphService.GetAppRolesByResourceIdAsync();
        if (!rolesResult.Successful)
            throw new InvalidOperationException($"Get transaction approval role failed: {rolesResult.Message}");

        var roles = rolesResult.Value;
        var role = roles.FirstOrDefault(r => r.DisplayName == notifySettings.VerificationRole);
        if (role == null)
            throw new InvalidOperationException("Transaction approval user role not found");

        var roleId = role.Id;
        var assignmentResult = await graphService.GetAppRoleAssignmentsByRoleAsync(roleId?.ToString());
        if (!assignmentResult.Successful)
            throw new InvalidOperationException($"Get transaction approval users failed: {assignmentResult.Message}");

        var userIds = await GetUserIdsFromRoleMembers(assignmentResult.Value);
        return await GetApprovalUsersFromUserIds(userIds);
    }

    private async Task<ICollection<ApprovalUser>> GetApprovalUsersFromUserIds(IEnumerable<Guid> userIds)
    {
        List<ApprovalUser> value = new();
        foreach (var userId in userIds)
        {
            var userResult = await graphService.GetUsersAsync(userId.ToString());
            var users = userResult.Value;

            foreach (var user in users)
            {
                var email = user.GetEmails().FirstOrDefault(e => e.Contains("@snco.us"))?.ToUpper();
                if (email == null)
                    continue;
                value.Add(new ApprovalUser
                {
                    EmailAddress = user.GetEmails().FirstOrDefault(e => e.Contains("@snco.us"))?.ToUpper(),
                    Id = Guid.Parse(user.Id),
                    DisplayName = user.GetName()?.ToUpper()
                });
            }
        }
        return value;
    }

    private async Task<ICollection<Guid>> GetUserIdsFromRoleMembers(IEnumerable<AppRoleAssignment> assignments)
    {
        var userIds = assignments
            .Where(a => a.PrincipalType == "User")
            .Select(a => a.PrincipalId.Value)
            .ToList();
        var groupIds = assignments
            .Where(a => a.PrincipalType == "Group")
            .Select(a => a.PrincipalId.Value)
            .ToList();
        List<Guid> searchedGroups = new();

        while (groupIds.Any())
        {
            List<Guid> newGroupIds = new();
            foreach (var groupId in groupIds)
            {
                var groupResult = await graphService.GetGroupsAsync(id: groupId.ToString(), includeMembers: true);
                if (!groupResult.Successful)
                    throw new InvalidOperationException($"Get group for transaction approval users failed: {groupResult.Message}");

                foreach (var group in groupResult.Value)
                {
                    var id = Guid.Parse(group.Id);
                    if (searchedGroups.Contains(id))
                        continue;
                    searchedGroups.Add(id);

                    foreach (var member in group.Members)
                    {
                        var memberId = Guid.Parse(member.Id);
                        switch (member.ODataType)
                        {
                            case "#microsoft.graph.user":
                                userIds.Add(memberId);
                                break;
                            case "#microsoft.graph.group":
                                newGroupIds.Add(memberId);
                                break;
                            default:
                                // do nothing
                                break;
                        }
                    }
                }

            }
            groupIds = newGroupIds;
        }

        return userIds;
    }

}
