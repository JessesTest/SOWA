using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.RouteType;

public class RouteTypeEditViewModel
{
    public int RouteTypeID { get; set; }

    [Required]
    [MaxLength(3)]
    [Display(Name = "Route Number")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid Route Number.")]
    public string RouteNumber { get; set; }

    [Required]
    [MaxLength(1)]
    [Display(Name = "Route Type")]
    public string Type { get; set; }
}
