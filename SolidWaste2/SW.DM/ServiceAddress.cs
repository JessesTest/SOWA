using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class ServiceAddress
{
    public ServiceAddress()
    {
        BillServiceAddresses = new HashSet<BillServiceAddress>();
        Containers = new HashSet<Container>();
        ServiceAddressNotes = new HashSet<ServiceAddressNote>();
        TransactionHoldings = new HashSet<TransactionHolding>();
        Transactions = new HashSet<Transaction>();
        WorkOrders = new HashSet<WorkOrder>();
    }

    public string CustomerType { get; set; }
    public int CustomerId { get; set; }
    public int Id { get; set; }
    public string ServiceType { get; set; }
    public string LocationNumber { get; set; }
    public string LocationName { get; set; }
    public string LocationContact { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? CancelDate { get; set; }
    public int PeaddressId { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }

    public virtual Customer Customer { get; set; }
    public virtual ICollection<BillServiceAddress> BillServiceAddresses { get; set; }
    public virtual ICollection<Container> Containers { get; set; }
    public virtual ICollection<ServiceAddressNote> ServiceAddressNotes { get; set; }
    public virtual ICollection<TransactionHolding> TransactionHoldings { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; }
    public virtual ICollection<WorkOrder> WorkOrders { get; set; }

    [NotMapped]
    public PE.DM.Address PEAddress { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ServiceAddress other &&
            Id == other.Id &&
            Id > 0;
    }

    public override int GetHashCode()
    {
        return $"ServiceAddress:{Id}".GetHashCode();
    }
}
