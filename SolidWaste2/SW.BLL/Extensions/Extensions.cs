using PE.DM;
using System.Text;

namespace SW.BLL.Extensions;

public static class Extensions
{
    public static string ToFullString(this Address self)
    {
        var sb = new StringBuilder();

        if (self.Number.HasValue)
            sb.Append(self.Number.Value.ToString()).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.Direction))
            sb.Append(self.Direction).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.StreetName))
            sb.Append(self.StreetName).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.Suffix))
            sb.Append(self.Suffix).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.Apt))
            sb.Append(self.Apt).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.City))
            sb.Append(self.City).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.State))
            sb.Append(self.State).Append(' ');
        if (!string.IsNullOrWhiteSpace(self.Zip))
            sb.Append(self.Zip);

        return sb.ToString().Trim();
    }
}
