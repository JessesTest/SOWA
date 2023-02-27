using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class PaymentPlan
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(128)]
    [ForeignKey("Customer"), Column(Order = 0)]
    public string CustomerType { get; set; }
    [ForeignKey("Customer"), Column(Order = 1)]
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    public ICollection<PaymentPlanDetail> Details { get; set; }

    public int Months { get; set; }

    public bool Canceled { get; set; }
    
    [Required]
    [StringLength(255)]
    public string AddToi { get; set; }
    public DateTime AddDateTime { get; set; }
    [StringLength(255)]
    public string ChgToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public bool DelFlag { get; set; }
    [StringLength(255)]
    public string DelToi { get; set; }
    public DateTime? DelDateTime { get; set; }

    [NotMapped]
    public string Status
    {
        get
        {
            if (DelFlag)
                return "Deleted";
            if (Canceled)
                return "Canceled";
            if (Details != null)
            {
                if (Details.Last().DueDate < DateTime.Now.Date)
                    return "Expired";
            }
            return "Active";
        }
    }
}
