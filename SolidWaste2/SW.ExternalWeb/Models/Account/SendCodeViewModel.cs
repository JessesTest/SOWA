using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Account
{
    public class SendCodeViewModel
    {
        [Display(Name = "Two Factor Authentication Provider")]
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
    }
}
