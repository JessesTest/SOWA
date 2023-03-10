namespace SW.DM;

public class TransactionCodeRule
{
    public int TransactionCodeRuleId { get; set; }
    public string Description { get; set; }
    public int? FormulaId { get; set; }
    public int? TransactionCodeId { get; set; }
    public int? ContainerCodeId { get; set; }
    public int? ContainerSubtypeId { get; set; }
    public int? ContainerNumDaysService { get; set; }
    public decimal? ContainerBillingSize { get; set; }
    public bool DeleteFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime? AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
    public int? TransactionId { get; set; }

    public virtual ContainerCode ContainerCode { get; set; }
    public virtual ContainerSubtype ContainerSubtype { get; set; }
    public virtual Formula Formula { get; set; }
    public virtual Transaction Transaction { get; set; }
    public virtual TransactionCode TransactionCode { get; set; }
}
