namespace SW.DM;

public class BillContainerDetail
{
    public int BillServiceAddressId { get; set; }
    public int BillContainerDetailId { get; set; }
    public string ContainerType { get; set; }
    public string ContainerDescription { get; set; }
    public DateTime ContainerEffectiveDate { get; set; }
    public DateTime? ContainerCancelDate { get; set; }
    public decimal RateAmount { get; set; }
    public string RateDescription { get; set; }
    public string DaysProratedMessage { get; set; }
    public decimal BillingSize { get; set; }
    public string DaysService { get; set; }
    public decimal ContainerCharge { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public int ContainerId { get; set; }
    public bool? PaidFull { get; set; }
    public decimal? Partial { get; set; }
    public int ObjectCode { get; set; }

    public virtual BillServiceAddress BillServiceAddress { get; set; }
    public virtual Container Container { get; set; }
}
