namespace SW.DM;

public class BillBlobs
{
    public int BillBlobId { get; set; }
    public int BillMasterId { get; set; }
    public int CustomerId { get; set; }
    public int TransactionId { get; set; }
    public DateTime BegDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public byte[] BillFile { get; set; }
    public bool DelFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime? AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
}
