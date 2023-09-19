namespace SW.DM;

public class EquipmentLegacy
{
    public int EquipmentLegacyId { get; set; }
    public int EquipmentNumber { get; set; }
    public string EquipmentYear { get; set; }
    public string EquipmentMake { get; set; }
    public string EquipmentModel { get; set; }
    public string EquipmentVin { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public int? Mileage { get; set; }
    public decimal? PurchaseAmt { get; set; }
    public decimal? MonthlyDepreciationAmt { get; set; }
    public string EquipmentDescription { get; set; }
    public DateTime? DateOfLastPm { get; set; }
    public int? MilesAtLastPm { get; set; }
    public string StandbyEquipmentFlag { get; set; }
    public bool DelFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
}
