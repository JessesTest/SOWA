using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.PaymentPlans;

public class PaymentPlanViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(128)]
    [Display(Name = "Customer Type")]
    public string CustomerType { get; set; }
    [Display(Name = "Customer Id")]
    public int CustomerId { get; set; }
    //public Customer Customer { get; set; }

    //public IEnumerable<PaymentPlanDetail> Details { get; set; }

    public int Months { get; set; }

    public bool Canceled { get; set; }

    [Display(Name = "Billed Amount Due")]
    public decimal TotalDue { get; set; }

    [Display(Name = "Est. Monthly Bill")]
    public decimal MonthlyTotal { get; set; }

    [Display(Name = "Plan Amount")]
    public decimal MonthlyPayment { get; set; }

    [Display(Name = "First Payment Due")]
    public DateTime FirstPaymentDate { get; set; }

    [Display(Name = "Customer")]
    public string FullName { get; set; }

    public string Status { get; set; }

    public string AddToi { get; set; }
    public DateTime AddDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
}
