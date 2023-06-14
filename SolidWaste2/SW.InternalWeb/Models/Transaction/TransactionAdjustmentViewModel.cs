using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Transaction;

public class TransactionAdjustmentViewModel
{
    [Display(Name = "Account Number")]
    public string CustomerID { get; set; }

    [Display(Name = "Name")]
    public string CustomerName { get; set; }

    [Required]
    [Display(Name = "Transaction Code")]
    public int TransactionCodeID { get; set; }

    [Required]
    [Display(Name = "Amount")]
    [RegularExpression(@"^[0-9$,\.\-]+$")]
    public string TransactionAmount { get; set; }

    [Display(Name = "Associated Transaction")]
    public int? AssociatedTransactionId { get; set; }

    [Display(Name = "Comment")]
    public string Comment { get; set; }

    public int Locks { get; set; }

    public string EmailAddress { get; set; }
}
