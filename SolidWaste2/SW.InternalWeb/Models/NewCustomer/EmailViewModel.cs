using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.NewCustomer;

public class EmailViewModel
{
    public int Type { get; set; }

    [Display(Name = "Email Address")]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; }
}
