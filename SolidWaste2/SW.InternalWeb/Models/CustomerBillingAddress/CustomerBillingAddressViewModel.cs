using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerBillingAddress;

public class CustomerBillingAddressViewModel
{
    public int CustomerId { get; set; }
    public int Id { get; set; }

    public IEnumerable<ValidAddressDto> Addresses;
    public int SelectIndex { get; set; }

    public bool Override { get; set; }

    public bool? Undeliverable { get; set; }

    [Required]
    [Display(Name = "Address")]
    [StringLength(64)]
    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }


    [StringLength(32)]
    public string City { get; set; }


    [RegularExpression("[a-zA-Z]{2}", ErrorMessage = "Invalid State")]
    [StringLength(2)]
    public string State { get; set; }


    [RegularExpression("[0-9]{5}(-[0-9]{4})?", ErrorMessage = "Invalid Zip")]
    [StringLength(10)]
    public string Zip { get; set; }
}
