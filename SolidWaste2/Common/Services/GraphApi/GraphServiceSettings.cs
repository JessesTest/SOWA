using Common.Services.Common;

namespace Common.Services.GraphApi;

public class GraphServiceSettings
{
    public string BaseAddress { get; set; }
    public string AADResourceId { get; set; }
    public string B2CResourceId { get; set; }
    public string AADDefaultAccessRoleId { get; set; }
    public string AppPrefix { get; set; }
    public string Scope { get; set; }

    public ApiRequest GetUsers { get; set; }
    public ApiRequest GetUsersByApp { get; set; }
    public ApiRequest GetAppRoleAssignmentsByUserId { get; set; }
    public ApiRequest GetAppRolesByResourceId { get; set; }
    public ApiRequest GrantAppRole { get; set; }
    public ApiRequest RevokeAppRole { get; set; }
    public ApiRequest GetGroups { get; set; }
    public ApiRequest AddGroup { get; set; }
    public ApiRequest UpdateGroup { get; set; }
    public ApiRequest DeleteGroup { get; set; }
    public ApiRequest AddUser { get; set; }
    public ApiRequest UpdateUser { get; set; }
}
