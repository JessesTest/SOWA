using Common.Services.Common;

namespace Common.Services.GraphApi;

public interface IGraphService
{
    Task<Result<List<Microsoft.Graph.User>>> GetUsersAsync(string principalId = null, string tenant = "AAD");
    Task<Result<List<Microsoft.Graph.User>>> GetUsersByAppAsync(string searchEmail = null, string tenant = "AAD");
    Task<Result<List<Microsoft.Graph.AppRoleAssignment>>> GetAppRoleAssignmentsByUserIdAsync(string principalId, string tenant = "AAD");
    Task<Result<List<Microsoft.Graph.AppRoleAssignment>>> GetAppRoleAssignmentsByRoleAsync(string appRoleId, string tenant = "AAD");
    Task<Result<List<Microsoft.Graph.AppRole>>> GetAppRolesByResourceIdAsync(string tenant = "AAD");
    Task<Result> GrantAssignmentAsync(string principalId, string tenant = "AAD");
    Task<Result> GrantAssignmentAsync(string principalId, string appRoleId, string tenant = "AAD");
    Task<Result> RevokeAssignmentAsync(string appRoleAssignmentId, string tenant = "AAD");
    Task<Result> SetRolesAsync(string principalId, string tenant = "AAD", params string[] roleIds);
    Task<Result<List<Microsoft.Graph.Group>>> GetGroupsAsync(string id = null, string tenant = "AAD", bool includeMembers = false, bool includeAppRoleAssignments = false);
    Task<Result> AddGroupAsync(string groupName, string tenant = "AAD");
    Task<Result> UpdateGroupAsync(string id, string groupName, string tenant = "AAD");
    Task<Result> DeleteGroupAsync(string id, string tenant = "AAD");
    Task<Result<Microsoft.Graph.User>> AddUserAsync(Microsoft.Graph.User user, string tenant = "AAD");
    Task<Result<Microsoft.Graph.User>> UpdateUserAsync(Microsoft.Graph.User user, string tenant = "AAD");
}
