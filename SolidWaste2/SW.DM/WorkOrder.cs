namespace SW.DM;

public class WorkOrder
{
    public string CustomerType { get; set; }
    public int? CustomerId { get; set; }
    public int WorkOrderId { get; set; }
    public DateTime? TransDate { get; set; }
    public DateTime? ResolveDate { get; set; }
    public string DriverInitials { get; set; }
    public string CustomerName { get; set; }
    public string CustomerAddress { get; set; }
    public int? ServiceAddressId { get; set; }
    public int? ContainerId { get; set; }
    public string ContainerCode { get; set; }
    public string ContainerRoute { get; set; }
    public decimal? ContainerSize { get; set; }
    public bool ContainerPickupMon { get; set; }
    public bool ContainerPickupTue { get; set; }
    public bool ContainerPickupWed { get; set; }
    public bool ContainerPickupThu { get; set; }
    public bool ContainerPickupFri { get; set; }
    public bool ContainerPickupSat { get; set; }
    public string RepairsNeeded { get; set; }
    public string ResolutionNotes { get; set; }
    public bool DelFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime? AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
    public bool RecyclingFlag { get; set; }

    public virtual Container Container { get; set; }
    public virtual Customer Customer { get; set; }
    public virtual ServiceAddress ServiceAddress { get; set; }
}
