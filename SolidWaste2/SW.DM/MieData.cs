using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

[Serializable]
public class MieData          //Multipurpose Internet Extensions cater Images 
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MieDataID { get; set; }
    [Required]
    [StringLength(255)]
    public string MieDataImageID { get; set; }
    public byte[] MieDataImage { get; set; }
    [StringLength(255)]
    public string MieDataImageType { get; set; }
    public int? MieDataImageSize { get; set; }
    [StringLength(255)]
    public string MieDataImageFileName { get; set; }
    public bool MieDataActive { get; set; }
    public bool MieDataDelete { get; set; }
    [StringLength(255)]
    public string AddToi { get; set; }
    public DateTime AddDateTime { get; set; }
    [StringLength(255)]
    public string ChgToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    [StringLength(255)]
    public string DelToi { get; set; }
    public DateTime? DelDateTime { get; set; }

}
