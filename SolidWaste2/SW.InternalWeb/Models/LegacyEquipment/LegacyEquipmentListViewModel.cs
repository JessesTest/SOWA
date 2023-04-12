using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.LegacyEquipment;

public class LegacyEquipmentListViewModel
{
    public int EquipmentLegacyID { get; set; }

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
}
