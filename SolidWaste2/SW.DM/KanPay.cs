namespace SW.DM;

public class KanPay
{
    public int KanPayId { get; set; }
    public string KanPayTokenId { get; set; }
    public string KanPayAmount { get; set; }
    public string KanPayCid { get; set; }
    public string KanPayCustomerType { get; set; }
    public string KanPayIfasObjectCode { get; set; }
    public bool KanPayDelete { get; set; }
    public string KanPayAddToi { get; set; }
    public string KanPayChgToi { get; set; }
    public string KanPayDelToi { get; set; }
    public DateTime KanPayAddDateTime { get; set; }
    public DateTime? KanPayChgDateTime { get; set; }
    public DateTime? KanPayDelDateTime { get; set; }

}
