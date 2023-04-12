using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PE.DM;
using System.Linq.Expressions;
using System.Text;

namespace SW.InternalWeb.Extensions;

public static class Extensions
{

    public static string FormatAddress(this Address a)
    {
        if (a == null)
            return "";

        var sb = new StringBuilder();
        if (a.StreetName == "PO BOX" && a.Number.HasValue)
        {
            sb.Append("PO BOX ").Append(a.Number.Value);
        }
        else
        {
            if (a.Number.HasValue && a.Number.Value > 0)
                sb.Append(a.Number.Value);
            if (!string.IsNullOrWhiteSpace(a.Direction))
                sb.Append(" ").Append(a.Direction);
            if (!string.IsNullOrWhiteSpace(a.StreetName))
                sb.Append(" ").Append(a.StreetName);
            if (!string.IsNullOrWhiteSpace(a.Suffix))
                sb.Append(" ").Append(a.Suffix);
            if (!string.IsNullOrWhiteSpace(a.Apt))
                sb.Append(" ").Append(a.Apt);
        }
        return sb.ToString().Trim();
    }

    public static HtmlString FormatMultiLine(this string str)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(str))
        {
            var stream = new System.IO.StringReader(str);
            string temp;
            bool first = true;
            while ((temp = stream.ReadLine()) != null)
            {
                if (first)
                    first = false;
                else
                    sb.Append("<br/>");
                sb.Append(System.Net.WebUtility.HtmlEncode(temp));
            }
        }
        return new HtmlString(sb.ToString());
    }
}
