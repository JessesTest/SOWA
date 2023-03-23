using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Manage
{
    public class BusinessProfileViewModel
    {
        [MaxLength(255)]
        [Required]
        [Display(Name = "Business Name")]
        public string Name { get; set; }

        [Url]
        [Display(Name = "Web URL")]
        public string Url { get; set; }

        [Display(Name = "Account Information")]
        public DateTime WhenCreated { get; set; }
    }
}
