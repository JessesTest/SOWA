using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Legacy;

public class LegacyBViewModel
{
    [Display(Name = "Equipment#")]
    public int EquipmentNumber { get; set; }

    [Display(Name = "Date Resolved")]
    public DateTime? ResolveDate { get; set; }

    public string Driver { get; set; }

    [Display(Name = "Problem#")]
    public int? ProblemNumber { get; set; }

    public string Route { get; set; }

    public int? Mileage { get; set; }

    [Display(Name = "Breakdown Code")]
    public string BreakdownCode { get; set; }

    [Display(Name = "Replace Equip#")]
    public int? ReplaceEuipmentNumber { get; set; }

    [Display(Name = "Breakdown Location")]
    public string BreakdownLocation { get; set; }

    [Display(Name = "Breakdown Description")]
    public string BreakdownDescription { get; set; }

    [Display(Name = "Add Date")]
    public DateTime AddDate { get; set; }

    [Display(Name = "Add Toi")]
    public string AddToi { get; set; }

    [Display(Name = "Update Date")]
    public DateTime? ChgDate { get; set; }

    [Display(Name = "Update Toi")]
    public string ChgToi { get; set; }

    public int? LegacyEquipmentId { get; set; }

    [Display(Name = "Trans Date")]
    public DateTime TransDate { get; set; }
}
