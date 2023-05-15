using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.NewCustomer;

public class ServiceAddressViewModel
{
    public Guid Id { get; set; }


    [Required]
    [Display(Name = "Location Name")]
    [StringLength(50)]
    public string LocationName { get; set; }

    [Display(Name = "Location Contact")]
    [StringLength(50)]
    public string LocationContact { get; set; }

    [Required]
    [Display(Name = "Effective Date")]
    public DateTime EffectiveDate { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

    [Display(Name = "Address")]
    [StringLength(64)]
    [Required]
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }

    [Required]
    [StringLength(32)]
    public string City { get; set; }

    [Required]
    [StringLength(2)]
    public string State { get; set; }

    //[Required]
    [RegularExpression("[0-9]{5}(-[0-9]{4})?")]
    [StringLength(10)]
    public string Zip { get; set; }

    public bool Override { get; set; }

    public ICollection<ContainerViewModel> Containers { get; set; } = new List<ContainerViewModel>();
    public ICollection<NoteViewModel> Notes { get; set; } = new List<NoteViewModel>();


    public ICollection<ValidAddressDto> Addresses { get; set; }
    public int AddressesIndex { get; set; }


    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        return hash.ToHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is ServiceAddressViewModel other &&
            other.Id == Id;
    }


    //[Display(Name = "Billing Amount")]
    //public decimal BillingAmount
    //{
    //    get
    //    {
    //        decimal total = 0;
    //        foreach (var c in Containers)
    //        {
    //            total += c.BillingAmount;
    //        }
    //        return total;
    //    }
    //}
}
