using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerServiceAddress;

public class AddressViewModel
{
    public int? Number { get; set; }

    [StringLength(255)]
    public string Direction { get; set; }

    [StringLength(255)]
    [Display(Name = "Street")]
    public string StreetName { get; set; }

    [StringLength(255)]
    public string Suffix { get; set; }

    [StringLength(255)]
    public string Apt { get; set; }

    [StringLength(255)]
    [Required]
    public string City { get; set; }

    [Required]
    [StringLength(255)]
    [RegularExpression(@"[a-zA-Z]{2}", ErrorMessage = "Invalid State")]
    public string State { get; set; }

    [StringLength(255)]
    [Required]
    [RegularExpression(@"[0-9]{5}(-[0-9]{4})?", ErrorMessage = "Invalid Zip")]
    public string Zip { get; set; }

    public bool Override { get; set; }
}
