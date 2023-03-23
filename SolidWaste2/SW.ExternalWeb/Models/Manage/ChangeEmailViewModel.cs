using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Manage
{
    public class ChangeEmailViewModel
    {
        public IEnumerable<SelectListItem> EmailTypesDropDown { get; set; }

        public List<EmailListViewModel> Emails { get; set; }

        public int EmailID { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Type")]
        public int EmailType { get; set; }
    }
}
