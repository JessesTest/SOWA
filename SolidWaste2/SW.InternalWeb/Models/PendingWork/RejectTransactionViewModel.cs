using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.PendingWork;

public class RejectTransactionViewModel
{
    [Required]
    public int? TransactionHoldingID { get; set; }
    public string Comment { get; set; }
}
