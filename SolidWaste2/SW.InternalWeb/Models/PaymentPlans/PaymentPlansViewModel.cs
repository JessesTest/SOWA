using SW.DM;

namespace SW.InternalWeb.Models.PaymentPlans;

public class PaymentPlansViewModel
{
    public int CustomerId { get; set; }
    public IEnumerable<PaymentPlan> PaymentPlans { get; set; }
}
