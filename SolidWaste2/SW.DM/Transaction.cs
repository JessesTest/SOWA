using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Transaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Customer"), Column(Order = 0)]
    public string CustomerType { get; set; }
    [ForeignKey("Customer"), Column(Order = 1)]
    public int CustomerID { get; set; }
    public Customer Customer { get; set; }

    public int? ContainerID { get; set; }
    [ForeignKey("ContainerID")]
    public Container Container { get; set; }

    public int? ServiceAddressID { get; set; }
    [ForeignKey("ServiceAddressID")]
    public ServiceAddress ServiceAddress { get; set; }

    public int? TransactionCodeId { get; set; }
    [ForeignKey("TransactionCodeId")]
    public TransactionCode TransactionCode { get; set; }
    
    public Int64? CheckNumber { get; set; }

    [StringLength(16)]
    public string WorkOrder { get; set; }

    [StringLength(100)]
    public string Comment { get; set; }

    public bool? PaidFull { get; set; }

    public decimal? Partial { get; set; }

    public int ObjectCode { get; set; }

    public int? TransactionHoldingId { get; set; }
    [ForeignKey("TransactionHoldingId")]
    public TransactionHolding TransactionHolding { get; set; }

    // current balance amount
    public decimal TransactionAmt { get; set; }
    public decimal CollectionsAmount { get; set; }
    public decimal CounselorsAmount { get; set; }
    public decimal? UncollectableAmount { get; set; }

    // current balance
    public decimal TransactionBalance { get; set; }
    public decimal CollectionsBalance { get; set; }
    public decimal CounselorsBalance { get; set; }
    public decimal? UncollectableBalance { get; set; }

    public int Sequence { get; set; }
    
    [ForeignKey("AssociatedTransaction")]
    public int? AssociatedTransactionId { get; set; }
    
    public Transaction AssociatedTransaction { get; set; }

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

    public virtual List<TransactionCodeRule> TransactionCodeRules { get; set; }
}
