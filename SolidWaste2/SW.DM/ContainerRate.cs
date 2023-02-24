using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class ContainerRate
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ContainerRateID { get; set; }
    
    public int ContainerType { get; set; }

    [ForeignKey("ContainerType")]
    public virtual ContainerCode Container { get; set; }

    public int ContainerSubtypeID { get; set; }
    [ForeignKey("ContainerSubtypeID")]
    public ContainerSubtype ContainerSubtypes { get; set; }        
    
    public decimal BillingSize { get; set; }        
    
    public int? NumDaysService { get; set; }

    [StringLength(50)]
    public string RateDescription { get; set; }

    public decimal RateAmount { get; set; }

    public decimal PullCharge { get; set; }

    //SCMB-243-New-Container-Rates-For-2022
    public decimal ExtraPickup { get; set; }

    public int ObjectCode { get; set; }

    [Column(TypeName = "Date")]        
    public DateTime EffectiveDate { get; set; }

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
