using Microsoft.AspNetCore.Mvc.Rendering;

namespace SW.InternalWeb.Extensions;

public static class Helpers
{
    public static List<SelectListItem> GenerateCustomerCodeSelectList()
    {
        return new List<SelectListItem>()
        {
            new SelectListItem { Value = "C", Text = "C - Commercial" },
            new SelectListItem { Value = "R", Text = "R - Residential" },
            new SelectListItem { Value = "H", Text = "H - Home Owners Association" }
        };
    }
}
