namespace SW.DM;

public class ContainerSubtype
{
    public ContainerSubtype()
    {
        ContainerRates = new HashSet<ContainerRate>();
        Containers = new HashSet<Container>();
        TransactionCodeRules = new HashSet<TransactionCodeRule>();
    }

    public int ContainerSubtypeId { get; set; }
    public int ContainerCodeId { get; set; }
    public string BillingFrequency { get; set; }
    public string Description { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }

    public virtual ContainerCode ContainerCode { get; set; }
    public virtual ICollection<ContainerRate> ContainerRates { get; set; }
    public virtual ICollection<Container> Containers { get; set; }
    public virtual ICollection<TransactionCodeRule> TransactionCodeRules { get; set; }
}
