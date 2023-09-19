namespace SW.DM;

public class CodeRef
{
    public int CodeId { get; set; }
    public int RefId { get; set; }
    public string Code { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public bool DelFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime? AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
}
