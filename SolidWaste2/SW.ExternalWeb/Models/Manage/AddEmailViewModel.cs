using Microsoft.AspNetCore.Mvc.Rendering;
using SW.ExternalWeb.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Manage
{
    public class AddEmailViewModel
    {
        public IEnumerable<SelectListItem> EmailTypesDropDown { get; set; }

        public List<EmailListViewModel> Emails { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Type")]
        public int EmailType { get; set; }
    }
}
