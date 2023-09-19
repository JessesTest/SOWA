namespace SW.DM;

public class ContainerRate
{
    public int ContainerRateId { get; set; }
    public int ContainerType { get; set; }
    public int ContainerSubtypeId { get; set; }
    public decimal BillingSize { get; set; }
    public int? NumDaysService { get; set; }
    public string RateDescription { get; set; }
    public decimal RateAmount { get; set; }
    public decimal PullCharge { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public int ObjectCode { get; set; }
    public decimal ExtraPickup { get; set; }

    public virtual ContainerSubtype ContainerSubtype { get; set; }
    public virtual ContainerCode ContainerTypeNavigation { get; set; }
}
