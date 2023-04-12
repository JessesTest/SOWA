using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Legacy;

public class LegacyRViewModel
{
    [Required]
    public int WorkOrderLegacyId { get; set; }

    [Display(Name = "Route#/Day")]
    [StringLength(5)]
    public string Route { get; set; }

    [Display(Name = "Date Resolved")]
    public DateTime? ResolveDate { get; set; }
    public bool AlreadyResolvedFlag { get; set; }

    [StringLength(4)]
    public string Driver { get; set; }

    [Display(Name = "Problem#")]
    public int? ProblemNumber { get; set; }

    public int? Mileage { get; set; }

    [Display(Name = "Problem Description")]
    [StringLength(225)]
    public string ProblemDescription { get; set; }

    [Display(Name = "Add Toi")]
    [StringLength(64)]
    public string AddToi { get; set; }

    [Display(Name = "Add Date")]
    public DateTime AddDate { get; set; }

    [Display(Name = "Update Toi")]
    [StringLength(64)]
    public string ChgToi { get; set; }

    [Display(Name = "Update Date")]
    public DateTime? ChgDate { get; set; }

    [Display(Name = "Trans Date")]
    public DateTime TransDate { get; set; }
}
