using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.WorkOrder;

public class WorkOrderViewModel
{
    [Display(Name = "Work Order Number")]
    public int? WorkOrderId { get; set; }

    [Display(Name = "Add Date")]
    public string TransDate { get; set; }

    [Display(Name = "Date Resolved")]
    public string ResolveDate { get; set; }

    [StringLength(4)]
    [Display(Name = "Driver *")]
    [Required]
    public string DriverInitials { get; set; }

    [Display(Name = "Customer Number")]
    public int? CustomerId { get; set; }

    [Display(Name = "Customer Type")]
    [Required]
    public string CustomerType { get; set; }

    [StringLength(64)]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; }

    [StringLength(128)]
    [Display(Name = "Customer Billing Address")]
    public string CustomerAddress { get; set; }

    [Display(Name = "Service Address")]
    public int? ServiceAddressId { get; set; }

    [Display(Name = "Container")]
    public int? ContainerId { get; set; }

    [StringLength(1)]
    [Display(Name = "Container Type *")]
    [Required]
    public string ContainerCode { get; set; }

    [Display(Name = "Recycling Container")]
    public bool RecyclingFlag { get; set; }

    [StringLength(5)]
    [Display(Name = "Route *")]
    [Required]
    public string ContainerRoute { get; set; }

    [Display(Name = "Container Size *")]
    [Required]
    public decimal? ContainerSize { get; set; }

    [Display(Name = "M")]
    public bool ContainerPickupMon { get; set; }

    [Display(Name = "T")]
    public bool ContainerPickupTue { get; set; }

    [Display(Name = "W")]
    public bool ContainerPickupWed { get; set; }

    [Display(Name = "Th")]
    public bool ContainerPickupThu { get; set; }

    [Display(Name = "F")]
    public bool ContainerPickupFri { get; set; }

    [Display(Name = "S")]
    public bool ContainerPickupSat { get; set; }

    [StringLength(256)]
    [Display(Name = "Repairs Needed *")]
    [Required]
    public string RepairsNeeded { get; set; }

    [StringLength(256)]
    [Display(Name = "Resolution Notes")]
    public string ResolutionNotes { get; set; }

    [StringLength(64)]
    [Display(Name = "Add Toi")]
    public string AddToi { get; set; }

    [StringLength(64)]
    [Display(Name = "Update Toi")]
    public string ChgToi { get; set; }

    [Display(Name = "Add DateTime")]
    public string AddDateTime { get; set; }

    [Display(Name = "Update DateTime")]
    public string ChgDateTime { get; set; }
}
