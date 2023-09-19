using System.Text;

namespace SW.ExternalWeb.Extensions
{
    public static class Extensions
    {
        public static string ToLine1String(this PE.DM.Address self)
        {
            if (self == null)
                return string.Empty;

            var sb = new StringBuilder();

            if (self.Number.HasValue)
            {
                sb.Append(self.Number.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(self.Direction))
            {
                sb.Append(" ");
                sb.Append(self.Direction);
            }

            if (!string.IsNullOrWhiteSpace(self.StreetName))
            {
                sb.Append(" ");
                sb.Append(self.StreetName);
            }

            if (!string.IsNullOrWhiteSpace(self.Suffix))
            {
                sb.Append(" ");
                sb.Append(self.Suffix);
            }

            if (!string.IsNullOrWhiteSpace(self.Apt))
            {
                sb.Append(" ");
                sb.Append(self.Apt);
            }

            return sb.ToString().Trim();
        }

        public static string ToLine2String(this PE.DM.Address self)
        {
            if (self == null)
                return string.Empty;

            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(self.City))
            {
                sb.Append(self.City);
            }

            if (!string.IsNullOrWhiteSpace(self.State))
            {
                sb.Append(" ");
                sb.Append(self.State);
            }

            if (!string.IsNullOrWhiteSpace(self.Zip))
            {
                sb.Append(" ");
                sb.Append(self.Zip);
            }

            return sb.ToString().Trim();
        }
    }
}
