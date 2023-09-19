using Notify.DM;

namespace Notify.BL.Services;

public interface INotifyService
{
    Task Add(Notification notification);
    Task DeleteById(int notificationId);
    Task<Notification> GetById(int id, bool includeDeleted = false);
    Task<ICollection<Notification>> GetByTo(string email);
    Task ToggleReadById(int notificationId);

    Task<int> GetUnreadCountByTo(string email);
    Task<ICollection<Notification>> GetUnreadByTo(string email, int limit);
    //Task<ICollection<ApprovalUser>> GetApprovalUsers()
}

public class NotifySettings
{
    public string VerificationRole { get; set; } = "role.admin";
}


//public class ApprovalUser
//{
//    public Guid Id { get; set; }
//    public string DisplayName { get; set; }
//    public string EmailAddress { get; set; }
//}
