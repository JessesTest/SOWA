using Microsoft.AspNetCore.Mvc.Rendering;

namespace SW.InternalWeb.Extensions;

public static class Helpers
{
    public static IEnumerable<SelectListItem> CustomerCodes =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "C", Text = "C - Commercial" },
            new SelectListItem { Value = "R", Text = "R - Residential" },
            new SelectListItem { Value = "H", Text = "H - Home Owners Association" }
        };

    public static IEnumerable<SelectListItem> EmailTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "7", Text = "P - Personal" },
            new SelectListItem { Value = "8", Text = "W - Work" }
        };

    public static IEnumerable<SelectListItem> EmailStatuses =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "true", Text = "Active" },
            new SelectListItem { Value = "false", Text = "Inactive" }
        };

    public static IEnumerable<SelectListItem> DeliveryOptions =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "1", Text = "Email only" },
            new SelectListItem { Value = "2", Text = "Paper only" },
            new SelectListItem { Value = "3", Text = "Both Email and Paper" }
        };

    public static IEnumerable<SelectListItem> PhoneTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "3", Text = "H - Home"},
            new SelectListItem { Value = "4", Text = "W - Work"},
            new SelectListItem { Value = "5", Text = "C - Cell"},
            new SelectListItem { Value = "6", Text = "F - Fax"}
        };

    public static IEnumerable<SelectListItem> PhoneStatuses =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "true", Text = "Active" },
            new SelectListItem { Value = "false", Text = "Inactive" }
        };
}
