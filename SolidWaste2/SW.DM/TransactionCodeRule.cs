using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class TransactionCodeRule
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionCodeRuleID { get; set; }

    [MaxLength(64)]
    public string Description { get; set; }

    [ForeignKey("Formula")]
    public int? FormulaID { get; set; }
    public virtual Formula Formula { get; set; }

    [ForeignKey("TransactionCode")]
    public int? TransactionCodeID { get; set; }
    public virtual TransactionCode TransactionCode { get; set; }

    [ForeignKey("ContainerCode")]
    public int? ContainerCodeID { get; set; }
    public virtual ContainerCode ContainerCode { get; set; }

    [ForeignKey("ContainerSubtype")]
    public int? ContainerSubtypeID { get; set; }
    public virtual ContainerSubtype ContainerSubtype { get; set; }

    public int? ContainerNumDaysService { get; set; }

    public decimal? ContainerBillingSize { get; set; }

    public bool DeleteFlag { get; set; }

    [MaxLength(64)]
    public string AddToi { get; set; }

    [MaxLength(64)]
    public string ChgToi { get; set; }

    [MaxLength(64)]
    public string DelToi { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? AddDateTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? ChgDateTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DelDateTime { get; set; }
}
