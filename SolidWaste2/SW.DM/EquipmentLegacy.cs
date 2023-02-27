using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class EquipmentLegacy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EquipmentLegacyId { get; set; }

    public int EquipmentNumber { get; set; }

    [StringLength(2)]
    public string EquipmentYear { get; set; }

    [StringLength(20)]
    public string EquipmentMake { get; set; }

    [StringLength(20)]
    public string EquipmentModel { get; set; }

    [StringLength(20)]
    public string EquipmentVIN { get; set; }

    [Column(TypeName = "date")]
    public DateTime? PurchaseDate { get; set; }

    public int? Mileage { get; set; }

    public decimal? PurchaseAmt { get; set; }

    public decimal? MonthlyDepreciationAmt { get; set; }

    [StringLength(20)]
    public string EquipmentDescription { get; set; }

    [Column(TypeName = "date")]
    public DateTime? DateOfLastPM { get; set; }

    public int? MilesAtLastPM { get; set; }

    [StringLength(2)]
    public string StandbyEquipmentFlag { get; set; }

    public bool DelFlag { get; set; }

    [StringLength(64)]
    public string AddToi { get; set; }

    [StringLength(64)]
    public string ChgToi { get; set; }

    [StringLength(64)]
    public string DelToi { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime AddDateTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? ChgDateTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DelDateTime { get; set; }
}
