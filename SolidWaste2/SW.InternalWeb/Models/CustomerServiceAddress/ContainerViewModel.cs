using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerServiceAddress;

public class ContainerViewModel
{
    public int Id { get; set; }

    [Display(Name = "Type")]
    public int ContainerCodeId { get; set; }

    [Display(Name = "Billing Size")]
    public decimal BillingSize { get; set; }

    [Display(Name = "Actual Size")]
    public decimal ActualSize { get; set; }

    [Display(Name = "Additional Charge")]
    [RegularExpression(@"^(?!\.?$)\d{0,6}(\.\d{0,2})?$")]
    public string AdditionalCharge { get; set; }

    [Display(Name = "Effective Date *")]
    public DateTime EffectiveDate { get; set; }

    [Display(Name = "Cancel Date")]
    public DateTime? CancelDate { get; set; }

    [Display(Name = "Route Number")]
    public string RouteNumber { get; set; }

    [Display(Name = "Status")]
    public string Delivered { get; set; }

    [Display(Name = "Mon")]
    public bool MonService { get; set; }

    [Display(Name = "Tue")]
    public bool TueService { get; set; }

    [Display(Name = "Wed")]
    public bool WedService { get; set; }

    [Display(Name = "Thu")]
    public bool ThuService { get; set; }

    [Display(Name = "Fri")]
    public bool FriService { get; set; }

    [Display(Name = "Sat")]
    public bool SatService { get; set; }

    public int DaysOfService
    {
        get
        {
            int count = 0;
            if (MonService)
                count++;
            if (TueService)
                count++;
            if (WedService)
                count++;
            if (ThuService)
                count++;
            if (FriService)
                count++;
            if (SatService)
                count++;
            return count;
        }
    }

    [Display(Name = "Subtype")]
    public int ContainerSubtypeID { get; set; }

    [Display(Name = "AddToi")]
    public string AddToi { get; set; }

    [Required]
    [Display(Name = "Add Date")]
    public DateTime AddDateTime { get; set; }

    [Display(Name = "ChgToi")]
    public string ChgToi { get; set; }

    [Display(Name = "Chg Date")]
    public DateTime? ChgDateTime { get; set; }
}
