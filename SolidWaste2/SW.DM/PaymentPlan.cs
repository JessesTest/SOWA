using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class PaymentPlan
{
    public PaymentPlan()
    {
        Details = new HashSet<PaymentPlanDetail>();
    }

    public string CustomerType { get; set; }
    public int CustomerId { get; set; }
    public int Id { get; set; }
    public int Months { get; set; }
    public bool Canceled { get; set; }
    public string AddToi { get; set; }
    public DateTime AddDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public bool DelFlag { get; set; }
    public string DelToi { get; set; }
    public DateTime? DelDateTime { get; set; }

    public virtual Customer Customer { get; set; }
    public virtual ICollection<PaymentPlanDetail> Details { get; set; }

    [NotMapped]
    public string Status
    {
        get
        {
            if (DelFlag)
                return "Deleted";
            if (Canceled)
                return "Canceled";
            if (Details != null && Details.Last().DueDate < DateTime.Now.Date)
                return "Expired";
            return "Active";
        }
    }
}
