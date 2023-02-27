using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class WarningLetter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Customer"), Column(Order = 0)]
    [StringLength(1)]
    public string CustomerType { get; set; }
    [ForeignKey("Customer"), Column(Order = 1)]
    public string CustomerId { get; set; }
    public Customer Customer { get; set; }

    public DateTime LetterDate { get; set; }

    public decimal Amount { get; set; }

    public int BillId { get; set; }
    [ForeignKey("BillId")]
    public Transaction Bill { get; set; }

    [StringLength(255)]
    public string SendTo { get; set; }
    [StringLength(255)]
    public string AddressLine1 { get; set; }
    [StringLength(255)]
    public string AddressLine2 { get; set; }

    public DateTime AddDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }

    public bool DeleteFlag { get; set; }
}
