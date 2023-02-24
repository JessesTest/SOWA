using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class PastDue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Days { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    [StringLength(255)]
    public string Address1 { get; set; }

    [Required]
    [StringLength(255)]
    public string Address2 { get; set; }

    [ForeignKey("TransactionId")]
    public Transaction Transaction { get; set; }
    public int TransactionId { get; set; }

    public DateTime ProcessDate { get; set; }

    public bool IsLetter { get; set; }

    public bool IsCollections { get; set; }

    public bool IsCounselors { get; set; }









    public DateTime AddDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }
}
