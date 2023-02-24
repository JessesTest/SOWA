using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class BillBlobs
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillBlobId { get; set; }
    
    public int BillMasterId { get; set; }

    public int CustomerID { get; set; }
    
    public int TransactionID { get; set; }

    public DateTime BegDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public byte[] BillFile { get; set; }

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
