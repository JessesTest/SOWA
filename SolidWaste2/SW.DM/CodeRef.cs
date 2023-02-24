using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class CodeRef
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CodeId { get; set; }

    public int RefId { get; set; }

    [StringLength(8)]
    public string Code { get; set; }

    [StringLength(8)]
    public string ShortDescription { get; set; }

    [StringLength(48)]
    public string LongDescription { get; set; }

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
