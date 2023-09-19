using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models;

public class HomePageViewModel
{
    public int CustomerId { get; set; }
    [DisplayFormat(DataFormatString = "{0:C0}")]
    public decimal PastDueBalance { get; set; }
    [DisplayFormat(DataFormatString = "{0:C0}")]
    public decimal MostRecentBillAmt { get; set; }
    public string AccountNumber { get; set; }
    [DisplayFormat(DataFormatString = "{0:C0}")]
    public decimal CurrentBalance { get; set; }
    public decimal? PPBalance { get; set; }
    public bool DOIHAVECOUNSELORS { get; set; }
}
