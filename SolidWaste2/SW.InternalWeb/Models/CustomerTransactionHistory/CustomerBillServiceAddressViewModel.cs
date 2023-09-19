namespace SW.InternalWeb.Models.CustomerTransactionHistory;

public class CustomerBillServiceAddressViewModel
{
    public List<CustomerBillContainerDetailViewModel> BillContainers { get; set; }
    public string BillMasterId { get; set; }
    public string BillServiceAddressId { get; set; }
    public string ServiceAddressName { get; set; }
    public string ServiceAddressStreet { get; set; }

}
