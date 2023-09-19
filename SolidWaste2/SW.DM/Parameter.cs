namespace SW.DM;

public class Parameter
{
    public int FormulaId { get; set; }
    public string ParameterId { get; set; }
    public string Name { get; set; }
    public decimal? Value { get; set; }
    public bool Constant { get; set; }
    public bool Delete { get; set; }
    public DateTime AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }

    public virtual Formula Formula { get; set; }
}
