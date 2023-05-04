using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.NewCustomer;

public class ContainerViewModel
{
    public Guid Id { get; set; }
    public Guid ServiceAddressId { get; set; }

    [Required]
    [Display(Name = "Container Type")]
    public int ContainerCodeId { get; set; }

    [Required]
    [Display(Name = "Container Subtype")]
    public int ContainerSubtypeId { get; set; }

    [Required]
    [Range(0, 99.9)]
    [Display(Name = "Billing Size")]
    public decimal BillingSize { get; set; }

    [Range(0, 99.9)]
    [Display(Name = "Actual Size")]
    public decimal ActualSize { get; set; }

    [Display(Name = "Additional Charge ±")]
    [RegularExpression(@"^-?[0-9]{0,6}(?:\.[0-9]{1,2})?$", ErrorMessage = "Invalid Additional Charge")]
    public decimal AdditionalCharge { get; set; }

    [Display(Name = "Route Number")]
    [StringLength(8)]
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

    [Required]
    [Display(Name = "Effective Date")]
    public DateTime EffectiveDate { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ContainerViewModel other
            && other.Id == Id;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        return hash.ToHashCode();
    }
}
