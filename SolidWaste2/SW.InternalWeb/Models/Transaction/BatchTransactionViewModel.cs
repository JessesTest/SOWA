using Microsoft.AspNetCore.Mvc.Rendering;
using SW.InternalWeb.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.InternalWeb.Models.Transaction;

public class BatchTransactionViewModel
{
    public IEnumerable<BatchTransactionElement> Transactions { get; set; } = new List<BatchTransactionElement>();
    public IEnumerable<SelectListItem> TransactionCodeSelectList { get; set; }
    public BatchTransactionElement CurrentTransaction { get; set; } = new BatchTransactionElement();
    public IDictionary<int, string> ActiveBatches { get; set; }

    [Display(Name = "Batch")]
    public int BatchID { get; set; }

    public class BatchTransactionElement
    {
        public string TransactionHoldingID { get; set; }

        [Display(Name = "Account Number")]
        public string CustomerID { get; set; }

        [Display(Name = "Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Transaction Code")]
        public string TransactionCodeID { get; set; }

        [Display(Name = "Amount")]
        public string TransactionAmt { get; set; }

        [Display(Name = "Check Number")]
        public string CheckNumber { get; set; }

        [Display(Name = "Work Order")]
        public string WorkOrder { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }
    }
}
