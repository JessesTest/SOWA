using System.ComponentModel.DataAnnotations;

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

    [StringLength(1)]
    [Required]
    public string Type { get; set; }

    [StringLength(50)]
    [Required]
    public string Description { get; set; }

    [StringLength(8)]
    public string BillingFrequency { get; set; }

    public bool DeleteFlag { get; set; }
    [Required]
    public DateTime AddDateTime { get; set; }
    [Required]
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
