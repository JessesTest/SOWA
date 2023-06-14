using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Customer
{
    public Customer()
    {
        BillMasters = new HashSet<BillMaster>();
        PaymentPlans = new HashSet<PaymentPlan>();
        ServiceAddresses = new HashSet<ServiceAddress>();
        TransactionHoldings = new HashSet<TransactionHolding>();
        Transactions = new HashSet<Transaction>();
        WorkOrders = new HashSet<WorkOrder>();
    }

    public string CustomerType { get; set; }
    public int CustomerId { get; set; }
    public int Pe { get; set; }
    public string NameAttn { get; set; }
    public string Contact { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? CancelDate { get; set; }
    public decimal? ContractCharge { get; set; }
    public string PurchaseOrder { get; set; }
    public string Notes { get; set; }
    public bool Active { get; set; }
    public DateTime AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string AddToi { get; set; } = null!;
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public int? LegacyCustomerId { get; set; }
    public bool PaymentPlan { get; set; }

    public virtual ICollection<BillMaster> BillMasters { get; set; }
    public virtual ICollection<PaymentPlan> PaymentPlans { get; set; }
    public virtual ICollection<ServiceAddress> ServiceAddresses { get; set; }
    public virtual ICollection<TransactionHolding> TransactionHoldings { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; }
    public virtual ICollection<WorkOrder> WorkOrders { get; set; }

    [NotMapped]
    public PE.DM.PersonEntity PersonEntity { get; set; }

    public override bool Equals(object obj)
    {
        return obj is Customer other &&
            other.CustomerType == CustomerType &&
            other.CustomerId == CustomerId &&
            CustomerId > 0;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(CustomerType);
        hash.Add(CustomerId);
        return hash.ToHashCode();
    }
}
