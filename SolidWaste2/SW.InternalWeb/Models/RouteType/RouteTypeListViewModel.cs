using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.RouteType;

public class RouteTypeListViewModel
{
    public int RouteTypeID { get; set; }

    [Display(Name = "Route Number")]
    public string RouteNumber { get; set; }

    [Display(Name = "Type")]
    public string Type { get; set; }
}
