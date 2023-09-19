using Microsoft.AspNetCore.Mvc.Rendering;
using SW.DM;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Transaction;

public class TransactionViewModel
{
    [Display(Name = "Account Number")]
    public string CustomerID { get; set; }

    [Display(Name = "Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Service Address")]
    public int ServiceAddressID { get; set; }

    [Display(Name = "Container")]
    public int ContainerID { get; set; }

    [Required]
    [Display(Name = "Transaction Code")]
    public int TransactionCodeID { get; set; }

    [Required]
    [Display(Name = "Amount")]
    [RegularExpression(@"^[0-9$,\.\-]+$")]
    public string TransactionAmount { get; set; }

    [Display(Name = "Work Order")]
    [StringLength(16)]
    public string WorkOrder { get; set; }

    [Display(Name = "Comment")]
    public string Comment { get; set; }

    public int Locks { get; set; }

    public Formula Formula { get; set; }
}
