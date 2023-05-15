using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.NewCustomer;

public class BillingAddressViewModel
{
    public IEnumerable<ValidAddressDto> Addresses { get; set; }
    public int SelectIndex { get; set; }

    public bool Override { get; set; }

    [Required]
    [Display(Name = "Address")]
    [StringLength(128)]
    public string AddressLine1 { get; set; }

    [Display(Name = "Apt")]
    [StringLength(32)]
    public string AddressLine2 { get; set; }

    [StringLength(32)]
    public string City { get; set; }

    [RegularExpression("[a-zA-Z]{2}", ErrorMessage = "Invalid State")]
    [StringLength(2)]
    public string State { get; set; }

    [RegularExpression("[0-9]{5}(-[0-9]{4})?", ErrorMessage = "Invalid Zip")]
    [StringLength(10)]
    public string Zip { get; set; }

    public string ApprovedAddress { get; set; }
    public void SetApproved()
    {
        SelectIndex = -1;
        Addresses = null;
        ApprovedAddress = $"{AddressLine1}:{AddressLine2}:{City}:{State}:{Zip}";
    }
    public bool IsApproved()
    {
        var temp = $"{AddressLine1}:{AddressLine2}:{City}:{State}:{Zip}";
        return temp == ApprovedAddress;
    }
}

public class ValidAddressDto
{
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
}
