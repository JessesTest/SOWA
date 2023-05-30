using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Writeoff;

public class WriteoffPaymentViewModel
{
    [Display(Name = "Customer ID")]
    [Required]
    public int? CustomerId { get; set; }

    [StringLength(255)]
    [Required]
    public string Name { get; set; }

    [Display(Name = "Past Due 90 Days")]
    public decimal PastDue90Days { get; set; }

    public decimal Collections { get; set; }

    public decimal Counselors { get; set; }

    public decimal Uncollectable { get; set; }

    public bool PaymentPlan { get; set; }

    [Required]
    [Display(Name = "Transaction Type")]
    public string TransactionCode { get; set; }

    public decimal Amount { get; set; }

    [StringLength(255)]
    public string Comment { get; set; }
}
