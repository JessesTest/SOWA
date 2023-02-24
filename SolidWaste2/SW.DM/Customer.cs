using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Customer
{
    [Key]
    [Column(Order = 0)]        
    public string CustomerType { get; set; }

    [Key]
    [Column(Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CustomerID { get; set; }

    public int PE { get; set; }

    public int? LegacyCustomerID { get; set; }

    [StringLength(255)]
    public string NameAttn { get; set; }

    [StringLength(255)]
    public string Contact { get; set; }

    public DateTime EffectiveDate { get; set; }

    [Display(Name = "Cancel Date")]
    public DateTime? CancelDate { get; set; }

    [Display(Name = "Contract Charge")]
    public decimal? ContractCharge { get; set; }

    [StringLength(16)]
    public string PurchaseOrder { get; set; }

    [StringLength(255)]
    public string Notes { get; set; }

    public bool PaymentPlan { get; set; }

    public bool Active { get; set; }

    [Required]
    public DateTime AddDateTime { get; set; }

    public DateTime? ChgDateTime { get; set; }

    public DateTime? DelDateTime { get; set; }

    [Required]
    [StringLength(255)]
    public string AddToi { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }

    [NotMapped]
    public PE.DM.PersonEntity PersonEntity { get; set; }
}
