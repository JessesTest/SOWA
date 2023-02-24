using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class BillContainerDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillContainerDetailId { get; set; }
    
    public BillServiceAddress BillServiceAddress { get; set; }

    [ForeignKey("BillServiceAddress"), Column(Order = 0)]
    public int BillServiceAddressId { get; set; }

    public Container Container { get; set; }

    [ForeignKey("Container"), Column(Order = 1)]
    public int ContainerId { get; set; }

    [StringLength(1)]
    public string ContainerType { get; set; }

    [StringLength(100)]
    public string ContainerDescription { get; set; }

    public DateTime ContainerEffectiveDate { get; set; }

    public DateTime? ContainerCancelDate { get; set; }

    public decimal RateAmount { get; set; }

    [StringLength(100)]
    public string RateDescription { get; set; }

    [StringLength(100)]
    public string DaysProratedMessage { get; set; }

    public decimal BillingSize { get; set; }

    [StringLength(6)]
    public string DaysService { get; set; }

    public decimal ContainerCharge { get; set; }

    public bool? PaidFull { get; set; }

    public decimal? Partial { get; set; }

    public int ObjectCode { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime AddDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }
}
