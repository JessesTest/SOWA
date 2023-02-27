using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class TransactionHolding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionHoldingId { get; set; }

    [ForeignKey("Customer"), Column(Order = 0)]
    public string CustomerType { get; set; }
    [ForeignKey("Customer"), Column(Order = 1)]
    public int CustomerID { get; set; }
    public Customer Customer { get; set; }

    public int? TransactionCodeId { get; set; }
    [ForeignKey("TransactionCodeId")]
    public TransactionCode TransactionCode { get; set; }

    public int? ServiceAddressId { get; set; }
    [ForeignKey("ServiceAddressId")]
    public ServiceAddress ServiceAddress { get; set; }

    public int? ContainerId { get; set; }
    [ForeignKey("ContainerId")]
    public Container Container { get; set; }

    public decimal TransactionAmt { get; set; }
    
    public Int64? CheckNumber { get; set; }

    [StringLength(16)]
    public string WorkOrder { get; set; }

    [StringLength(100)]
    public string Comment { get; set; }

    [StringLength(32)]
    public string Status { get; set; }

    [StringLength(255)]
    public string Sender { get; set; }

    public string Security { get; set; }

    [StringLength(255)]
    public string Approver { get; set; }

    public int BatchID { get; set; }

    public int ObjectCode { get; set; }
    
    public int? AssociatedTransactionId { get; set; }

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
