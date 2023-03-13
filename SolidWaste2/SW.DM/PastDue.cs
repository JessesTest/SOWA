namespace SW.DM;

public class PastDue
{
    public int Id { get; set; }
    public int Days { get; set; }
    public decimal Amount { get; set; }
    public string Name { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public int TransactionId { get; set; }
    public DateTime ProcessDate { get; set; }
    public bool IsLetter { get; set; }
    public bool IsCollections { get; set; }
    public bool IsCounselors { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }

    public virtual Transaction Transaction { get; set; }
}
