using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerBillingAddress;

public class CustomerBillingAddressViewModel
{
    public string CustomerType { get; set; }
    public int CustomerID { get; set; }
    public int? Id { get; set; }

    public IList<CustomerBillingAddressViewModel> Addresses;
    public int SelectIndex { get; set; }

    public bool Override { get; set; }

    public bool? Undeliverable { get; set; }

    public string Number { get; set; }
    public string Direction { get; set; }
    public string StreetName { get; set; }
    public string Suffix { get; set; }
    public string Apt { get; set; }

    [Required]
    [Display(Name = "Address")]
    [StringLength(64)]
    public string AddressLine1 { get; set; }


    [StringLength(32)]
    public string City { get; set; }


    [RegularExpression("[a-zA-Z]{2}", ErrorMessage = "Invalid State")]
    [StringLength(2)]
    public string State { get; set; }


    [RegularExpression("[0-9]{5}(-[0-9]{4})?", ErrorMessage = "Invalid Zip")]
    [StringLength(10)]
    public string Zip { get; set; }
}
