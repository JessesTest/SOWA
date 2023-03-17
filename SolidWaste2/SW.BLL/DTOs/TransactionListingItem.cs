namespace SW.BLL.DTOs;

public class TransactionListingItem
{
    public int TransactionID { get; set; }
    public string TransactionCode { get; set; }
    public DateTime AddDateTime { get; set; }
    public decimal TransactionAmount { get; set; }
    public decimal TransactionBalance { get; set; }
    public string Comment { get; set; }
}
