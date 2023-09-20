using System.Security.Claims;

namespace Common.Extensions;

public static class UserExtensions
{
    private const string defaultUsername = "unknown";

    public static string GetUserId(this ClaimsPrincipal user)
    {
        // uid
        // these are different in azure ad (but not in b2c ??)
        // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
        // http://schemas.microsoft.com/identity/claims/objectidentifier

        return user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }

    public static string GetName(this ClaimsPrincipal user)
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
    }

    public static string GetFirstName(this ClaimsPrincipal user)
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
    }

    public static string GetLastName(this ClaimsPrincipal user)
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        if (user == null)
            return null;

        var email = user.FindFirst("emails")?.Value;
        if (!string.IsNullOrWhiteSpace(email))
            return email;

        var pun = user?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
        if(pun != null && pun.EndsWith("@sncoapps.us"))
        {
            // currently no graph api access
            // what can go wrong with this?
            var index = pun.IndexOf("@sncoapps.us");
            var name = pun.Substring(0, index);
            return $"{name}@snco.us";
        }

        return pun;
    }

    public static string GetNameOrEmail(this ClaimsPrincipal user)
    {
        if (user == null)
            return defaultUsername;

        var name = GetName(user);
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        var email = GetEmail(user);
        if (!string.IsNullOrWhiteSpace(email))
            return email;

        var id = GetUserId(user);
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        return defaultUsername;
    }
}
