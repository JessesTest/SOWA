using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class WorkOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Work Order Number")]
    public int WorkOrderId { get; set; }

    [Column(TypeName = "date")]
    public DateTime? TransDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? ResolveDate { get; set; }

    [StringLength(4)]
    public string DriverInitials { get; set; }

    [ForeignKey("Customer"), Column(Order = 1)]
    public int? CustomerId { get; set; }
    public Customer Customer { get; set; }

    [ForeignKey("Customer"), Column(Order = 0)]
    public string CustomerType { get; set; }

    [StringLength(64)]
    public string CustomerName { get; set; }

    [StringLength(128)]
    public string CustomerAddress { get; set; }

    public bool RecyclingFlag { get; set; }

    public int? ServiceAddressId { get; set; }
    [ForeignKey("ServiceAddressId")]
    public ServiceAddress ServiceAddress { get; set; }

    public int? ContainerId { get; set; }
    [ForeignKey("ContainerId")]
    public Container Container { get; set; }

    [StringLength(1)]
    public string ContainerCode { get; set; }

    [StringLength(5)]
    public string ContainerRoute { get; set; }

    public decimal? ContainerSize { get; set; }

    public bool ContainerPickupMon { get; set; }

    public bool ContainerPickupTue { get; set; }

    public bool ContainerPickupWed { get; set; }

    public bool ContainerPickupThu { get; set; }

    public bool ContainerPickupFri { get; set; }

    public bool ContainerPickupSat { get; set; }

    [StringLength(256)]
    public string RepairsNeeded { get; set; }
    
    [StringLength(256)]
    public string ResolutionNotes { get; set; }

    public bool DelFlag { get; set; }

    [StringLength(64)]
    public string AddToi { get; set; }
    
    [StringLength(64)]
    public string ChgToi { get; set; }
    
    [StringLength(64)]
    public string DelToi { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime? AddDateTime { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime? ChgDateTime { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime? DelDateTime { get; set; }
}
