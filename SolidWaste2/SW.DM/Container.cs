using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Container
{
    public Container()
    {
        BillContainerDetails = new HashSet<BillContainerDetail>();
        TransactionHoldings = new HashSet<TransactionHolding>();
        Transactions = new HashSet<Transaction>();
        WorkOrders = new HashSet<WorkOrder>();
    }

    public int Id { get; set; }
    public int ServiceAddressId { get; set; }
    public int ContainerCodeId { get; set; }
    public int ContainerSubtypeId { get; set; }
    public decimal BillingSize { get; set; }
    public decimal ActualSize { get; set; }
    public decimal AdditionalCharge { get; set; }
    public bool MonService { get; set; }
    public bool TueService { get; set; }
    public bool WedService { get; set; }
    public bool ThuService { get; set; }
    public bool FriService { get; set; }
    public bool SatService { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? CancelDate { get; set; }
    public string RouteNumber { get; set; }
    public string Delivered { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public bool PrepaidFlag { get; set; }

    public virtual ContainerCode ContainerCode { get; set; }
    public virtual ContainerSubtype ContainerSubtype { get; set; }
    public virtual ServiceAddress ServiceAddress { get; set; }
    public virtual ICollection<BillContainerDetail> BillContainerDetails { get; set; }
    public virtual ICollection<TransactionHolding> TransactionHoldings { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; }
    public virtual ICollection<WorkOrder> WorkOrders { get; set; }

    [NotMapped]
    public int NumDaysService
    {
        get
        {
            int temp = 0;
            if (MonService)
                temp++;
            if (TueService)
                temp++;
            if (WedService)
                temp++;
            if (ThuService)
                temp++;
            if (FriService)
                temp++;
            if (SatService)
                temp++;
            return temp;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is Container other &&
            Id == other.Id &&
            Id > 0;
    }

    public override int GetHashCode()
    {
        return $"Container:{Id}".GetHashCode();
    }
}
