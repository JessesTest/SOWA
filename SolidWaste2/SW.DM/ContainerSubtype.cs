using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class ContainerSubtype
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ContainerSubtypeID { get; set; }

    public int ContainerCodeId { get; set; }
    [ForeignKey("ContainerCodeId")]
    public ContainerCode ContainerCode { get; set; }

    [StringLength(10)]
    public string BillingFrequency { get; set; }

    [StringLength(40)]
    public string Description { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime AddDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }
}
