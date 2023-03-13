using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Account
{
    public class EmailConfirmationViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
