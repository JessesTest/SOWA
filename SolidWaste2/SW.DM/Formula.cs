namespace SW.DM;

public class Formula
{
    public Formula()
    {
        Parameters = new HashSet<Parameter>();
        TransactionCodeRules = new HashSet<TransactionCodeRule>();
    }

    public int FormulaId { get; set; }
    public string Name { get; set; }
    public string FormulaString { get; set; }
    public bool Delete { get; set; }
    public DateTime AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public string CommentString { get; set; }

    public virtual ICollection<Parameter> Parameters { get; set; }
    public virtual ICollection<TransactionCodeRule> TransactionCodeRules { get; set; }
}
