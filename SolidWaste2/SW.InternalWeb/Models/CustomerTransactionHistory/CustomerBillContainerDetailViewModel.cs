namespace SW.InternalWeb.Models.CustomerTransactionHistory;

public class CustomerBillContainerDetailViewModel
{
    public string BillServiceAddressId { get; set; }
    public string BillContainerDetailId { get; set; }
    public string ContainerType { get; set; }
    public string ContainerDescription { get; set; }
    public string ContainerEffectiveDate { get; set; }
    public string ContainerCancelDate { get; set; }
    public string RateAmount { get; set; }
    public string RateDescription { get; set; }
    public string DaysProratedMessage { get; set; }
    public string DaysService { get; set; }
    public string ContainerCharge { get; set; }
}
