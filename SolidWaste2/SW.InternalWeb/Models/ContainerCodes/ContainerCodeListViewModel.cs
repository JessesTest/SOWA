using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.ContainerCodes;

public class ContainerCodeListViewModel
{
    public IEnumerable<SelectListItem> BillingFrequencies { get; set; }

    public int ContainerCodeID { get; set; }

    [Display(Name = "Code")]
    public string Type { get; set; }

    [Display(Name = "Code Description")]
    public string Description { get; set; }
}
