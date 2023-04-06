using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Collections;

public class CollectionsPaymentViewModel
{
    [Display(Name = "Customer ID")]
    [Required]
    public int? CustomerId { get; set; }

    [StringLength(255)]
    [Required]
    public string Name { get; set; }

    public decimal Collections { get; set; }

    public decimal Counselors { get; set; }

    public decimal Uncollectable { get; set; }

    public bool PaymentPlan { get; set; }

    [Display(Name = "Transaction Type")]
    public string TransactionCode { get; set; }

    public decimal Amount { get; set; }

    [StringLength(255)]
    public string Comment { get; set; }
}
