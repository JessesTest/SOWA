using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.WorkOrderInquiry;

public class WorkOrderInquiryViewModel
{
    [Display(Name = "Work Order Number")]
    public int? WorkOrderId { get; set; }

    [Display(Name = "Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Address")]
    public string CustomerAddress { get; set; }

    [Display(Name = "Route")]
    public string ContainerRoute { get; set; }

    [Display(Name = "Date")]
    public DateTime? TransDate { get; set; }

    [Display(Name = "Date")]
    public DateTime? ResolveDate { get; set; }

    [Display(Name = "Driver")]
    public string DriverInitials { get; set; }

    public bool Include { get; set; }

    public IEnumerable<InquiryResult> Results { get; set; }

    public class InquiryResult
    {
        public int WorkOrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        //public int CustomerId { get; set; }
        public string ContainerRoute { get; set; }
        public DateTime? TransDate { get; set; }
        public DateTime? ResolveDate { get; set; }
        public string DriverInitials { get; set; }
    }
}
