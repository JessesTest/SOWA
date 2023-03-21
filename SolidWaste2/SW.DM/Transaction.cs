namespace SW.DM;

public class Transaction
{
    public Transaction()
    {
        BillMasters = new HashSet<BillMaster>();
        InverseAssociatedTransaction = new HashSet<Transaction>();
        PastDues = new HashSet<PastDue>();
        TransactionCodeRules = new HashSet<TransactionCodeRule>();
    }

    public string CustomerType { get; set; }
    public int CustomerId { get; set; }
    public int Id { get; set; }
    public int? TransactionCodeId { get; set; }
    public long? CheckNumber { get; set; }
    public string Comment { get; set; }
    public int? TransactionHoldingId { get; set; }
    public decimal TransactionAmt { get; set; }
    public decimal CollectionsAmount { get; set; }
    public decimal CounselorsAmount { get; set; }
    public decimal TransactionBalance { get; set; }
    public decimal CollectionsBalance { get; set; }
    public decimal CounselorsBalance { get; set; }
    public int Sequence { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public string WorkOrder { get; set; }
    public bool? PaidFull { get; set; }
    public decimal? Partial { get; set; }
    public int ObjectCode { get; set; }
    public int? ContainerId { get; set; }
    public int? ServiceAddressId { get; set; }
    public decimal? UncollectableAmount { get; set; }
    public decimal? UncollectableBalance { get; set; }
    public int? AssociatedTransactionId { get; set; }

    public virtual Transaction AssociatedTransaction { get; set; }
    public virtual Container Container { get; set; }
    public virtual Customer Customer { get; set; }
    public virtual ServiceAddress ServiceAddress { get; set; }
    public virtual TransactionCode TransactionCode { get; set; }
    public virtual TransactionHolding TransactionHolding { get; set; }
    public virtual ICollection<BillMaster> BillMasters { get; set; }
    public virtual ICollection<Transaction> InverseAssociatedTransaction { get; set; }
    public virtual ICollection<PastDue> PastDues { get; set; }
    public virtual ICollection<TransactionCodeRule> TransactionCodeRules { get; set; }
}
