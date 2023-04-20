using SW.InternalWeb.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerPhone;

public class CustomerPhoneNumberViewModel
{

    public string CustomerType { get; set; }

    public int CustomerID { get; set; }

    public string FullName { get; set; }

    public int? Id { get; set; }

    public int Type { get; set; }


    //[RegularExpression(@"(\d\d\d) \d\d\d-\d\d\d\d")]
    [RegularExpression(@"^\(\d{3}\) \d{3}-\d{4}$", ErrorMessage = "Invalid Phone Number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Required]
    public bool Status { get; set; }

    public int CurrentIndex { get; set; }

    public int MaxIndex { get; set; }
}
