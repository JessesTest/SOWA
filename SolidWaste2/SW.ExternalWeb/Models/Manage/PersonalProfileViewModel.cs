using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Manage
{
    public class PersonalProfileViewModel
    {
        public IEnumerable<SelectListItem> SexSelectList { get; set; }

        [MaxLength(255)]
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [MaxLength(255)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [MaxLength(255)]
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(255)]
        [Display(Name = "Sex")]
        public string Sex { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Url]
        [Display(Name = "Web URL")]
        public string Url { get; set; }

        public DateTime WhenCreated { get; set; }

        public PersonalProfileViewModel()
        {
            SexSelectList = new List<SelectListItem>()
            {
                new SelectListItem { Value = "", Text = "Please choose your sex" },
                new SelectListItem { Value = "Male", Text = "Male" },
                new SelectListItem { Value = "Female", Text = "Female" }
            };
        }
    }
}
