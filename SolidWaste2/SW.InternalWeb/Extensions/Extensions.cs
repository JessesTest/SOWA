using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using PE.DM;
using SW.DM;
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
                sb.Append(' ').Append(a.Direction);
            if (!string.IsNullOrWhiteSpace(a.StreetName))
                sb.Append(' ').Append(a.StreetName);
            if (!string.IsNullOrWhiteSpace(a.Suffix))
                sb.Append(' ').Append(a.Suffix);
            if (!string.IsNullOrWhiteSpace(a.Apt))
                sb.Append(' ').Append(a.Apt);
        }
        return sb.ToString().Trim();
    }
    public static string FormatAddressLine1(this Address a)
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
                sb.Append(' ').Append(a.Direction);
            if (!string.IsNullOrWhiteSpace(a.StreetName))
                sb.Append(' ').Append(a.StreetName);
            if (!string.IsNullOrWhiteSpace(a.Suffix))
                sb.Append(' ').Append(a.Suffix);
        }
        return sb.ToString().Trim();
    }
    public static string FormatAddressLine2(this Address a)
    {
        return a?.Apt ?? "";
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
                    sb.Append("<br />");
                sb.Append(System.Net.WebUtility.HtmlEncode(temp));
            }
        }
        return new HtmlString(sb.ToString());
    }

    public static string Ellipsis(this string self, int length)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < length && i < self.Length; i++)
            sb.Append(self[i]);

        if (sb.Length >= length)
            sb.Append("...");

        return sb.ToString();
    }

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

    public static int DaysCount(this Models.NewCustomer.ContainerViewModel model)
    {
        var count = 0;

        if (model == null)
            return count;

        if (model.MonService) count++;
        if (model.TueService) count++;
        if (model.WedService) count++;
        if (model.ThuService) count++;
        if (model.FriService) count++;
        if (model.SatService) count++;
        
        return count;
    }

    public static bool IsAjaxRequest(this HttpRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException("request");
        }

        //return (request["X-Requested-With"] == "XMLHttpRequest") || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest"))
        return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    public static void AddXAlertMessage(this HttpResponse response, string message)
    {
        if (response == null)
            return;

        response.Headers.TryAdd("X-Alert-Message", message);
    }
    public static string GetTransactionCodeSignDisplayName(this string code)
    {
        return code switch
        {
            "P" => "P (+) - Positive",
            "N" => "N (-) - Negative",
            "B" => "B (+|-) - Both",
            //"" => "N/A",
            _ => "N/A",
        };
    }

    public static string GetTransactionCodeAccountTypeDisplayName(this string code)
    {
        return code switch
        {
            "B" => "B - Balance Type",
            "M" => "M - Money Type",
            "R" => "R - Receivable Type",
            _ => string.Empty,
        };
    }

    public static string GetTransactionCodeGroupTypeDisplayName(this string code)
    {
        return code switch
        {
            "S" => "S - Service",
            "M" => "M - Miscellaneous",
            "P" => "P - Payment",
            //"A" => ,                  //SOWA-93  No logic for displaying "A" group code existed in the original
            _ => string.Empty,
        };
    }

    public static string GetTransactionCodeDisplayName(this string code, IEnumerable<SelectListItem> codes)
    {
        foreach (var item in codes)
        {
            if (code == item.Value)
                return item.Text;
        }
        return string.Empty;
    }
}
