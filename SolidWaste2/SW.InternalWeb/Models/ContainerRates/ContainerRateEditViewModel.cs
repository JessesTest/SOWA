using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.ContainerRates;

public class ContainerRateEditViewModel
{
    public int ContainerRateID { get; set; }

    [Required]
    [Display(Name = "Container Code | Description")]
    public int ContainerType { get; set; }

    [Display(Name = "Subtype")]
    public int ContainerSubtype { get; set; }

    [Required]
    [Display(Name = "Bill Size")]
    [RegularExpression(@"^(\d{0,2})?(\.(\d{1}))?$", ErrorMessage = "Invalid Billing Size.")]
    [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
    public decimal? BillingSize { get; set; }

    [Required]
    [Display(Name = "Days Svc")]
    public int? NumDaysService { get; set; }

    [Required]
    [Display(Name = "Rate Description")]
    [MaxLength(50)]
    public string RateDescription { get; set; }

    [Required]
    [Display(Name = "Rate Amt")]
    [RegularExpression(@"^\$?((\d{1,2}\,\d{3})|(\d{0,5}))?(\.(\d{1,2}))?$", ErrorMessage = "Invalid Rate Amt.")]
    public string RateAmount { get; set; }

    [Required]
    [Display(Name = "Pull Charge")]
    [RegularExpression(@"^\$?((\d{1,2}\,\d{3})|(\d{0,5}))?(\.(\d{1,2}))?$", ErrorMessage = "Invalid Pull Charge.")]
    public string PullCharge { get; set; }

    [Required]
    [Display(Name = "Effective Date")]
    //[RegularExpression(@"^((((0?[1-9])|(1[0-2]))\/((0?[1-9])|([1-2][0-9])|3[0-1])\/[0-9]{4}))$", ErrorMessage = "Enter Effective Date in MM/DD/YYYY Format.")]
    public DateTime? EffectiveDate { get; set; }
}
