using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class KanPay
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int KanPayID { get; set; }
    [Required]
    [StringLength(255)]
    public string KanPayTokenID { get; set; }
    [StringLength(255)]
    public string KanPayAmount { get; set; }
    [StringLength(255)]
    public string KanPayCid { get; set; }        
    [StringLength(255)]
    public string KanPayCustomerType { get; set; }
    [StringLength(255)]
    public string KanPayIfasObjectCode { get; set; }
    public bool KanPayDelete { get; set; }
    [StringLength(255)]
    public string KanPayAddToi { get; set; }
    [StringLength(255)]
    public string KanPayChgToi { get; set; }
    [StringLength(255)]
    public string KanPayDelToi { get; set; }
    public DateTime KanPayAddDateTime { get; set; }
    public DateTime? KanPayChgDateTime { get; set; }
    public DateTime? KanPayDelDateTime { get; set; }
  
}
