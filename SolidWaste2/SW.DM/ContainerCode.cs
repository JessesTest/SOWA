namespace SW.DM;

public class ContainerCode
{
    public ContainerCode()
    {
        ContainerRates = new HashSet<ContainerRate>();
        ContainerSubtypes = new HashSet<ContainerSubtype>();
        Containers = new HashSet<Container>();
        TransactionCodeRules = new HashSet<TransactionCodeRule>();
    }

    public int ContainerCodeId { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string BillingFrequency { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }

    public virtual ICollection<ContainerRate> ContainerRates { get; set; }
    public virtual ICollection<ContainerSubtype> ContainerSubtypes { get; set; }
    public virtual ICollection<Container> Containers { get; set; }
    public virtual ICollection<TransactionCodeRule> TransactionCodeRules { get; set; }
}
