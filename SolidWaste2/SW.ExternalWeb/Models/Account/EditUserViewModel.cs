using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Account;

public class EditUserViewModel
{
    public string Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; }

    public IEnumerable<SelectListItem> RolesList { get; set; }
}
