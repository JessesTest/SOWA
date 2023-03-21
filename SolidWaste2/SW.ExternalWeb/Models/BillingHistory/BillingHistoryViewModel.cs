namespace SW.ExternalWeb.Models.BillingHistory;

public class BillingHistoryViewModel
{
    public string CustomerID { get; set; }

    public List<BillingHistoryListViewModel> Transactions { get; set; }
}
