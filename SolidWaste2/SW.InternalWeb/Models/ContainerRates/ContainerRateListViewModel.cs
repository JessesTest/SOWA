using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.ContainerRates;

public class ContainerRateListViewModel
{
    public int ContainerRateID { get; set; }

    [Display(Name = "Container Code | Description")]
    public int ContainerType { get; set; }

    [Display(Name = "Subtype")]
    public int ContainerSubtype { get; set; }

    [Display(Name = "Bill Size")]
    [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
    public decimal? BillingSize { get; set; }

    [Display(Name = "Days Svc")]
    public int? NumDaysService { get; set; }

    [Display(Name = "Rate Description")]
    public string RateDescription { get; set; }

    [Display(Name = "Rate Amt")]
    public decimal? RateAmount { get; set; }

    [Display(Name = "Pull Charge")]
    public decimal? PullCharge { get; set; }

    [Display(Name = "Effective Date")]
    public DateTime? EffectiveDate { get; set; }
}
