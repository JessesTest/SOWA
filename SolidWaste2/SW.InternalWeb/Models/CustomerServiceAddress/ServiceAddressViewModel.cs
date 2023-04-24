using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerServiceAddress;

public class ServiceAddressViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Service Effective Date")]
    public DateTime EffectiveDate { get; set; }

    [Display(Name = "Cancel Date")]
    public DateTime? CancelDate { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Location Name")]
    public string LocationName { get; set; }

    [StringLength(50)]
    [Display(Name = "Contact")]
    public string LocationContact { get; set; }

    [Display(Name = "Address Validation Override")]
    public bool AddressOverride { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Address")]
    public string AddressLine { get; set; }

    [Required]
    [StringLength(50)]
    public string City { get; set; }

    [Required]
    [StringLength(50)]
    public string State { get; set; }

    [Required]
    [StringLength(10)]
    public string Zip { get; set; }

    [Required]
    public string CustomerType { get; set; }
    [Required]
    public int CustomerId { get; set; }

    [RegularExpression("[^0-9]*([2-9][0-9]{2})[^0-9]*([2-9][0-9]{2})[^0-9]*([0-9]{4})[^0-9]*([0-9]{1,4}$)?")]
    public string Phone { get; set; }

    //SCMB-243-New-Container-Rates-For-2022
    //[RegularExpression(@"[^@]+@[^@]+\.[^@]+")]
    //[RegularExpression(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
    //@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
    //ErrorMessage = "Invalid Email")]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; }
}
