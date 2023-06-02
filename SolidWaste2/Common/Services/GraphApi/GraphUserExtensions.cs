using Microsoft.Graph;

namespace Common.Services.GraphApi;

public static class GraphUserExtensions
{
    public static string GetName(this User user)
    {
        var displayName = user.DisplayName;
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        return $"{user.GivenName} {user.Surname}";
    }

    public static IEnumerable<string> GetEmails(this User user)
    {
        List<string> emails = new();

        if (!string.IsNullOrWhiteSpace(user.Mail))
            emails.Add(user.Mail);

        emails.AddRange(user.Identities
            .Where(i => i.SignInType == "emailAddress" && !string.IsNullOrWhiteSpace(i.IssuerAssignedId))
            .Select(i => i.IssuerAssignedId)
            .ToList());

        if (user.OtherMails != null && user.OtherMails.Any())
            emails.AddRange(user.OtherMails);

        var set = new HashSet<string>(emails);
        return set.ToArray();
    }
}
