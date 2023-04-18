using Microsoft.AspNetCore.Mvc.Rendering;

namespace SW.InternalWeb.Extensions;

public static class Helpers
{
    public static List<SelectListItem> CustomerCodes
    {
        get
        {
            return new List<SelectListItem>()
            {
                new SelectListItem { Value = "C", Text = "C - Commercial" },
                new SelectListItem { Value = "R", Text = "R - Residential" },
                new SelectListItem { Value = "H", Text = "H - Home Owners Association" }
            };
        }
    }

    public static List<SelectListItem> EmailTypes
    {
        get
        {
            return new List<SelectListItem>()
            {
                new SelectListItem { Value = "7", Text = "P - Personal" },
                new SelectListItem { Value = "8", Text = "W - Work" }
            };
        }
    }

    public static List<SelectListItem> EmailStatuses
    {
        get
        {
            return new List<SelectListItem>()
            {
                new SelectListItem { Value = "true", Text = "Active" },
                new SelectListItem { Value = "false", Text = "Inactive" }
            };
        }
    }

    public static IEnumerable<SelectListItem> DeliveryOptions => new List<SelectListItem>()
    {
        new SelectListItem { Value = "1", Text = "Email only" },
        new SelectListItem { Value = "2", Text = "Paper only" },
        new SelectListItem { Value = "3", Text = "Both Email and Paper" }
    };
}
