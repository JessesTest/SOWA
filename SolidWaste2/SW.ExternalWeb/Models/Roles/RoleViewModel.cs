using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Roles;

public class RoleViewModel
{
    public string Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [Display(Name = "RoleName")]
    public string Name { get; set; }
}
