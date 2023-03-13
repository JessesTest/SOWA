namespace SW.DM;

public class TransactionHolding
{
    public TransactionHolding()
    {
        Transactions = new HashSet<Transaction>();
    }

    public string CustomerType { get; set; }
    public int CustomerId { get; set; }
    public int TransactionHoldingId { get; set; }
    public int? TransactionCodeId { get; set; }
    public decimal TransactionAmt { get; set; }
    public long? CheckNumber { get; set; }
    public string Comment { get; set; }
    public string Status { get; set; }
    public string Sender { get; set; }
    public string Approver { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public string WorkOrder { get; set; }
    public string Security { get; set; }
    public int BatchId { get; set; }
    public int? ServiceAddressId { get; set; }
    public int? ContainerId { get; set; }
    public int ObjectCode { get; set; }
    public int? AssociatedTransactionId { get; set; }

    public virtual Container Container { get; set; }
    public virtual Customer Customer { get; set; }
    public virtual ServiceAddress ServiceAddress { get; set; }
    public virtual TransactionCode TransactionCode { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; }
}
