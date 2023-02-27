using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class WorkOrderLegacy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int WorkOrderLegacyId { get; set; }

    [StringLength(1)]
    public string RecType { get; set; }

    public int EquipmentNumber { get; set; }

    [Column(TypeName = "date")]
    public DateTime TransDate { get; set; }
    
    [Column(TypeName = "date")]
    public DateTime? ResolveDate { get; set; }

    [StringLength(5)]
    public string StartTime { get; set; }

    [StringLength(5)]
    public string EndTime { get; set; }

    [StringLength(5)]
    public string Route { get; set; }

    [StringLength(4)]
    public string Driver { get; set; }

    public int? ProblemNumber { get; set; }

    public int? Mileage { get; set; }

    [StringLength(225)]
    public string ProblemDescription { get; set; }

    [StringLength(2)]
    public string BreakdownCode { get; set; }

    public int? ReplacementEquipmentNumber { get; set; }

    [StringLength(20)]
    public string BreakdownLocation { get; set; }

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
