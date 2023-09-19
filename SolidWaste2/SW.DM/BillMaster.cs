namespace SW.DM;

public class BillMaster
{
    public BillMaster()
    {
        BillServiceAddresses = new HashSet<BillServiceAddress>();
    }

    public string CustomerType { get; set; }
    public int CustomerId { get; set; }
    public int BillMasterId { get; set; }
    public int TransactionId { get; set; }
    public decimal? ContractCharge { get; set; }
    public string BillingName { get; set; }
    public string BillingAddressStreet { get; set; }
    public string BillingAddressCityStateZip { get; set; }
    public DateTime BillingPeriodBegDate { get; set; }
    public DateTime BillingPeriodEndDate { get; set; }
    public decimal PastDueAmt { get; set; }
    public decimal ContainerCharges { get; set; }
    public bool FinalBill { get; set; }
    public string BillMessage { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }

    public virtual Customer Customer { get; set; }
    public virtual Transaction Transaction { get; set; }
    public virtual ICollection<BillServiceAddress> BillServiceAddresses { get; set; }
}
