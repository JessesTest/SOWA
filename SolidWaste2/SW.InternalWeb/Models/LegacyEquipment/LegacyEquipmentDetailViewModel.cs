using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.LegacyEquipment;

public class LegacyEquipmentDetailViewModel
{
    public int EquipmentLegacyID { get; set; }

    [Required]
    [Display(Name = "Equipment #")]
    public int EquipmentNumber { get; set; }

    [Display(Name = "Year")]
    public string EquipmentYear { get; set; }

    [Display(Name = "Make")]
    public string EquipmentMake { get; set; }

    [Display(Name = "Model")]
    public string EquipmentModel { get; set; }

    [Display(Name = "VIN")]
    public string EquipmentVIN { get; set; }

    [Display(Name = "Purchase Date")]
    public string PurchaseDate { get; set; }

    [Display(Name = "Mileage")]
    public int? Mileage { get; set; }

    [Display(Name = "Purchase Amt")]
    public string PurchaseAmt { get; set; }

    [Display(Name = "Monthly Depreciation Amt")]
    public string MonthlyDepreciationAmt { get; set; }

    [Display(Name = "Equipment Description")]
    public string EquipmentDescription { get; set; }

    [Display(Name = "Date of Last PM")]
    public string DateOfLastPM { get; set; }

    [Display(Name = "Miles at Last PM")]
    public int? MilesAtLastPM { get; set; }

    [Display(Name = "Standby Equipment Flag")]
    public string StandbyEquipmentFlag { get; set; }

    [Display(Name = "Add Date")]
    public string AddDateTime { get; set; }

    [Display(Name = "Add Toi")]
    public string AddToi { get; set; }

    [Display(Name = "Update Date")]
    public string ChgDateTime { get; set; }

    [Display(Name = "Update Toi")]
    public string ChgToi { get; set; }
}
