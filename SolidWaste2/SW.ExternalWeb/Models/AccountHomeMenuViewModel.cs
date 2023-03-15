namespace SW.ExternalWeb.Models;

public class AccountHomeMenuViewModel
{
    public string CustomerType { get; set; }
    public string AccountNumber { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime DueDate { get; set; }
    public bool? Paperless { get; set; }
    public decimal MostRecentBillAmt { get; set; }
    public decimal ActivitySinceLastBill { get; set; }
    public decimal PastDueBalance { get; set; }
    public string CurrentController { get; set; }
    public string CurrentAction { get; set; }
    public string KanPayPayment { get; set; }
    public string KanPayPaymentChoice { get; set; }
    public bool PPFlag { get; set; }
    public decimal? PPBalance { get; set; }
}
