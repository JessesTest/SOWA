using SW.BLL.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.WorkOrderLegacyInquiry;

public class WorkOrderLegacyInquiryViewModel
{
    [Display(Name = "Equipment Number")]
    public int? EquipmentNumber { get; set; }

    [Display(Name = "Address")]
    public string BreakdownLocation { get; set; }

    [Display(Name = "Route")]
    public string Route { get; set; }

    [Display(Name = "Date")]
    public DateTime? TransDate { get; set; }

    [Display(Name = "Date")]
    public DateTime? ResolveDate { get; set; }

    [Display(Name = "Driver")]
    public string Driver { get; set; }

    [Display(Name = "Problem Number")]
    public int? ProblemNumber { get; set; }

    public string RecType { get; set; }

    public bool Include { get; set; }

    public IEnumerable<InquiryResult> Results { get; set; }

    public class InquiryResult
    {
        public int WorkOrderLegacyId { get; set; }
        public int EquipmentNumber { get; set; }
        public string BreakdownLocation { get; set; }
        public string Route { get; set; }
        public DateTime TransDate { get; set; }
        public DateTime? ResolveDate { get; set; }
        public string Driver { get; set; }
        public int? ProblemNumber { get; set; }
        public string RecType { get; set; }
    }
}
