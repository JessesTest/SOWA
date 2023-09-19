namespace SW.DM;

public class WorkOrderLegacy
{
    public int WorkOrderLegacyId { get; set; }
    public int EquipmentNumber { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime? ResolveDate { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Route { get; set; }
    public string Driver { get; set; }
    public int? ProblemNumber { get; set; }
    public int? Mileage { get; set; }
    public string ProblemDescription { get; set; }
    public string BreakdownCode { get; set; }
    public int? ReplacementEquipmentNumber { get; set; }
    public string BreakdownLocation { get; set; }
    public bool DelFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string RecType { get; set; }
}
