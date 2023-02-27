using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Container
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ServiceAddressId { get; set; }
    [ForeignKey("ServiceAddressId")]
    public ServiceAddress ServiceAddress { get; set; }

    public int ContainerCodeId { get; set; }
    [ForeignKey("ContainerCodeId")]
    public ContainerCode ContainerCode { get; set; }

    public int ContainerSubtypeID { get; set; }
    [ForeignKey("ContainerSubtypeID")]
    public ContainerSubtype ContainerSubtype { get; set; }

    public decimal BillingSize { get; set; }

    public decimal ActualSize { get; set; }

    public decimal AdditionalCharge { get; set; }

    public bool MonService { get; set; }
    public bool TueService { get; set; }
    public bool WedService { get; set; }
    public bool ThuService { get; set; }
    public bool FriService { get; set; }
    public bool SatService { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? CancelDate { get; set; }

    [StringLength(50)]
    public string RouteNumber { get; set; }

    [StringLength(255)]
    public string Delivered { get; set; }

    public virtual IList<BillContainerDetail> Bills { get; set; }

    public bool PrepaidFlag { get; set; }

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

    [NotMapped]
    public int NumDaysService
    {
        get
        {
            int temp = 0;
            if (MonService)
                temp++;
            if (TueService)
                temp++;
            if (WedService)
                temp++;
            if (ThuService)
                temp++;
            if (FriService)
                temp++;
            if (SatService)
                temp++;
            return temp;
        }
    }
}
