using Common.Services.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Common.Services.GraphApi;

public class GraphService : ApiServiceBase, IGraphService
{
    private readonly GraphServiceSettings _settings;
    private readonly ILogger<GraphService> _logger;

    public GraphService(IOptions<GraphServiceSettings> options, IOptions<AuthSettings> authSettings, ILogger<GraphService> logger)
    : base(options?.Value?.BaseAddress, options?.Value?.Scope, authSettings?.Value, logger)
    {
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<User>>> GetUsersAsync(string principalId = null, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, principalId, tenant };
            return await MakeRequest<List<User>>(_settings.GetUsers, obj);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error getting graph users");
            return new Result<List<User>>
            {
                Message = "Error getting graph users: " + e.Message,
                Successful = false
            };
        }
    }

    public async Task<Result<List<User>>> GetUsersByAppAsync(string searchEmail = null, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, searchEmail, tenant };
            return await MakeRequest<List<User>>(_settings.GetUsersByApp, obj);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting graph users of app");
            return new Result<List<User>>
            {
                Message = "Error getting graph users of app",
                Successful = false
            };
        }
    }

    public async Task<Result<List<AppRoleAssignment>>> GetAppRoleAssignmentsByUserIdAsync(string principalId, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, principalId, tenant };
            return await MakeRequest<List<AppRoleAssignment>>(_settings.GetAppRoleAssignmentsByUserId, obj);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting graph role assignments");
            return new Result<List<AppRoleAssignment>>
            {
                Message = "Error getting graph role assignments",
                Successful = false,
                Value = new List<AppRoleAssignment> ()
            };
        }
    }

    public async Task<Result<List<AppRoleAssignment>>> GetAppRoleAssignmentsByRoleAsync(string appRoleId, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, appRoleId, tenant };
            return await MakeRequest<List<AppRoleAssignment>>(_settings.GetAppRoleAssignmentsByUserId, obj);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting graph assignments");
            return new Result<List<AppRoleAssignment>>
            {
                Message = "Error getting graph assignments",
                Successful = false,
                Value = new List<AppRoleAssignment>()
            };
        }
    }

    public async Task<Result<List<AppRole>>> GetAppRolesByResourceIdAsync(string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, tenant };
            return await MakeRequest<List<AppRole>>(_settings.GetAppRolesByResourceId, obj);
        }
        catch(Exception e)
        {
            _logger.LogWarning(e, "Error getting graph roles");
            return new Result<List<AppRole>>
            {
                Message = "Error getting graph roles",
                Successful = false,
                Value = new List<AppRole>()
            };
        }
    }

    public async Task<Result> GrantAssignmentAsync(string principalId, string tenant = "AAD")
    {
        var appRoleId = _settings.AADDefaultAccessRoleId;
        return await GrantAssignmentAsync(principalId, appRoleId, tenant);
    }

    public async Task<Result> GrantAssignmentAsync(string principalId, string appRoleId, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, principalId, appRoleId, tenant };
            var result = await MakeRequest(_settings.GrantAppRole, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error granting role");
            return new Result
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result> RevokeAssignmentAsync(string appRoleAssignmentId, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, appRoleAssignmentId, tenant };
            return await MakeRequest(_settings.RevokeAppRole, obj);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error revoking assignment");
            return new Result
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result> SetRolesAsync(string principalId, string tenant = "AAD", params string[] roleIds)
    {
        var rolesToAdd = new List<string>();
        var assignmentsToRevoke = new List<string>();
        var newRoles = roleIds ?? Array.Empty<string>();
        try
        {
            var oldAssignments = await GetAppRoleAssignmentsByUserIdAsync(principalId, tenant);
            if (!oldAssignments.Successful)
                return oldAssignments;

            var oldRoleIds = oldAssignments.Value.Where(a => a.Id != null && a.AppRoleId != null).Select(a => a.AppRoleId.ToString());

            foreach (var assignment in oldAssignments.Value)
            {
                if (assignment.Id == null || assignment.AppRoleId == null)
                    continue;

                if (!newRoles.Contains(assignment.AppRoleId.ToString()))
                    assignmentsToRevoke.Add(assignment.Id);
            }
            foreach(var roleId in newRoles)
            {
                if(roleId != null && !oldRoleIds.Contains(roleId))
                {
                    rolesToAdd.Add(roleId);
                }
            }

            //--------------------------------------------------------------

            foreach (var roleId in rolesToAdd)
            {
                var grantResult = await GrantAssignmentAsync(principalId, roleId, tenant);
                if (!grantResult.Successful)
                    return new Result
                    {
                        Successful = false,
                        Message = $"Error granting role {roleId}: {grantResult.Message}" 
                    };
            }

            foreach(var assignment in assignmentsToRevoke)
            {
                var revokeResult = await RevokeAssignmentAsync(assignment, tenant);
                if (!revokeResult.Successful)
                    return new Result
                    {
                        Successful = false,
                        Message = $"Error revoking role assignment {assignment}: {revokeResult.Message}"
                    };
            }

            return new Result { Successful = true };
        }
        catch(Exception e)
        {
            _logger.LogWarning(e, "Error setting roles");
            return new Result
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result<List<Group>>> GetGroupsAsync(string id = null, string tenant = "AAD", bool includeMembers = false, bool includeAppRoleAssignments = false)
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, id, appPrefix = _settings.AppPrefix, includeMembers, includeAppRoleAssignments, tenant };
            var result = await MakeRequest<List<Group>>(_settings.GetGroups, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error getting groups");
            return new Result<List<Group>>
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result> AddGroupAsync(string groupName, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, appPrefix = _settings.AppPrefix, groupName, tenant };
            var result = await MakeRequest(_settings.AddGroup, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error adding group");
            return new Result
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result> UpdateGroupAsync(string id, string groupName, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, id, appPrefix = _settings.AppPrefix, groupName, tenant };
            var result = await MakeRequest(_settings.UpdateGroup, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error updating group");
            return new Result
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result> DeleteGroupAsync(string id, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, id, tenant };
            var result = await MakeRequest(_settings.DeleteGroup, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error deleting group");
            return new Result
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result<User>> AddUserAsync(User user, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, appPrefix = _settings.AppPrefix, tenant, user };
            var result = await MakeRequest<User>(_settings.AddUser, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error adding user");
            return new Result<User>
            {
                Successful = false,
                Message = e.Message
            };
        }
    }

    public async Task<Result<User>> UpdateUserAsync(User user, string tenant = "AAD")
    {
        try
        {
            var resourceId = tenant == "B2C"
                ? _settings.B2CResourceId
                : _settings.AADResourceId;
            var obj = new { resourceId, appPrefix = _settings.AppPrefix, tenant, user };
            var result = await MakeRequest<User>(_settings.UpdateUser, obj);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Update adding user");
            return new Result<User>
            {
                Successful = false,
                Message = e.Message
            };
        }
    }
}
