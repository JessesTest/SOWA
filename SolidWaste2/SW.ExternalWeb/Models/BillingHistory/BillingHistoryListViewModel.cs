using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.BillingHistory;

public class BillingHistoryListViewModel
{
    public string TransactionID { get; set; }

    public string TransactionCode { get; set; }

    [Display(Name = "Add Date Time")]
    public DateTime AddDateTime { get; set; }

    [Display(Name = "Transaction Code | Description")]
    public string Description { get; set; }

    [Display(Name = "Balance Forward")]
    public string BalanceForward { get; set; }

    [Display(Name = "Transaction Amount")]
    public string TransactionAmount { get; set; }

    [Display(Name = "Balance")]
    public string TransactionBalance { get; set; }
}
