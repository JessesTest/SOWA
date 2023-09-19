using System.ComponentModel.DataAnnotations;

namespace SW.DM;

public class PaymentPlanDetail
{
    public int Id { get; set; }
    public int PaymentPlanId { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaymentTotal { get; set; }
    public bool? Paid { get; set; }
    public bool Caneled { get; set; }
    public string AddToi { get; set; }
    public DateTime AddDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public bool DelFlag { get; set; }
    public string DelToi { get; set; }
    public DateTime? DelDateTime { get; set; }

    public virtual PaymentPlan PaymentPlan { get; set; }
}
