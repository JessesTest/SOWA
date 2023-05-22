using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Support;

public class SupportViewModel
{
    [Display(Name = "Customer Id")]
    public int? CustomerId { get; set; }

    public int? PersonEntityId { get; set; }

    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Confirmation Flag")]
    public bool EmailConfirmationFlag { get; set; }

    public bool HasOnline { get; set; }

    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    //[DataType(DataType.Password)]
    //[Display(Name = "Confirm password")]
    //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //public string ConfirmPassword { get; set; }
}
