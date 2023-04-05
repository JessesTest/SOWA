using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.ContainerCodes;

public class ContainerCodeAddViewModel
{
    //public IEnumerable<SelectListItem> BillingFrequencies { get; set; }

    [Required]
    [MaxLength(1)]
    [Display(Name = "Code")]
    [RegularExpression(@"^(([a-z]|[A-Z]){1})$", ErrorMessage = "Invalid Container Code.")]
    public string Type { get; set; }

    [Required]
    [MaxLength(50)]
    [Display(Name = "Code Description")]
    public string Description { get; set; }
}
