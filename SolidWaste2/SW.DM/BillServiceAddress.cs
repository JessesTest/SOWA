namespace SW.DM;

public class BillServiceAddress
{
    public BillServiceAddress()
    {
        BillContainerDetails = new HashSet<BillContainerDetail>();
    }

    public int BillMasterId { get; set; }
    public int BillServiceAddressId { get; set; }
    public string ServiceAddressName { get; set; }
    public string ServiceAddressStreet { get; set; }
    public string ServiceAddressCityStateZip { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public int ServiceAddressId { get; set; }

    public virtual BillMaster BillMaster { get; set; }
    public virtual ServiceAddress ServiceAddress { get; set; }
    public virtual ICollection<BillContainerDetail> BillContainerDetails { get; set; }
}
