using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class ContainerCode
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ContainerCodeID { get; set; }

    [StringLength(1)]
    public string Type { get; set; }
    
    [StringLength(50)]
    public string Description { get; set; }

    [StringLength(8)]
    public string BillingFrequency { get; set; }

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
