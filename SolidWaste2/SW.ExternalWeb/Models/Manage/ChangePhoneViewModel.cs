using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Manage
{
    public class ChangePhoneViewModel
    {
        public IEnumerable<SelectListItem> PhoneTypesDropDown { get; set; }

        public List<PhoneListViewModel> Phones { get; set; }

        public int PhoneID { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\(\d{3}\) \d{3}-\d{4}$", ErrorMessage = "Invalid Phone Number")]
        public string Number { get; set; }

        [Display(Name = "Extension")]
        public string Ext { get; set; }

        [Display(Name = "Type")]
        public int PhoneType { get; set; }
    }
}
