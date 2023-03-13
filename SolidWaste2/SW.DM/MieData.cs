namespace SW.DM;

[Serializable]
public class MieData          //Multipurpose Internet Extensions cater Images 
{
    public int MieDataId { get; set; }
    public string MieDataImageId { get; set; } = null!;
    public byte[] MieDataImage { get; set; }
    public string MieDataImageType { get; set; }
    public int? MieDataImageSize { get; set; }
    public string MieDataImageFileName { get; set; }
    public bool MieDataActive { get; set; }
    public bool MieDataDelete { get; set; }
    public string AddToi { get; set; }
    public DateTime AddDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string DelToi { get; set; }
    public DateTime? DelDateTime { get; set; }
}
