namespace SW.DM;

public class RouteType
{
    public int RouteTypeId { get; set; }
    public int RouteNumber { get; set; }
    public string Type { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
}
