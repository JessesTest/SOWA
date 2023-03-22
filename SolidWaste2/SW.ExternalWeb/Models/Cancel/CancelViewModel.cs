using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Cancel
{
    public class CancelViewModel
    {
        public string AccountNumber { get; set; }

        public string Phone { get; set; }

        public string BillingAddress1 { get; set; }

        public string BillingAddress2 { get; set; }

        [Required]
        public DateTime? CancelDate { get; set; }

        public DateTime MaxCancelDate
        {
            get
            {
                return DateTime.Now.Date.AddDays(14);
            }
        }

        public int? ServiceAddressId { get; set; }

        public int? ContainerId { get; set; }
    }
}
