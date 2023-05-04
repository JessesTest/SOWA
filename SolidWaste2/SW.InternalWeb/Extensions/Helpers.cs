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

    public static IEnumerable<SelectListItem> CustomerTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "", Text = "All" },
            new SelectListItem { Value = "C", Text = "Commercial" },
            new SelectListItem { Value = "R", Text = "Residential" },
            new SelectListItem { Value = "H", Text = "Home Owners Association" }
        };

    public static IEnumerable<SelectListItem> NewCustomerTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Value = "C", Text = "Commercial" },
            new SelectListItem { Value = "R", Text = "Residential" },
            new SelectListItem { Value = "H", Text = "Home Owners Association" }
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

    public static IEnumerable<SelectListItem> TransactionCodeSigns =>
        new SelectListItem[]
        {
            new SelectListItem { Text = "P (+) - Positive", Value = "P" },
            new SelectListItem { Text = "N (-) - Negative", Value = "N" },
            new SelectListItem { Text = "B (+|-) - Both", Value = "B" },
            new SelectListItem { Text = "  N/A", Value = "" }
        };

    public static IEnumerable<SelectListItem> TransactionCodeAccountTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Text = "B - Balance Type", Value = "B" },
            new SelectListItem { Text = "M - Money Type", Value = "M" },
            new SelectListItem { Text = "R - Receivable Type", Value = "R" }
        };

    public static IEnumerable<SelectListItem> TransactionCodeGroupTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Text = "" },
            new SelectListItem { Text = "S - Service", Value = "S" },
            new SelectListItem { Text = "M - Miscellaneous", Value = "M" },
            new SelectListItem { Text = "P - Payment", Value = "P" }
        };

    public static IEnumerable<SelectListItem> ContainerDeliveredTypes =>
        new SelectListItem[]
        {
            new SelectListItem { Text = " ", Value = " " },
            new SelectListItem { Text = "Scheduled for Delivery", Value = "Scheduled for Delivery" },
            new SelectListItem { Text = "Delivered", Value = "Delivered" },
            new SelectListItem { Text = "Customer Container", Value = "Customer Container" },
            new SelectListItem { Text = "Rejected", Value = "Rejected" },
            new SelectListItem { Text = "Returned", Value = "Returned" },
            new SelectListItem { Text = "Scheduled for Pick Up", Value = "Scheduled for Pick Up" }
        };
}
