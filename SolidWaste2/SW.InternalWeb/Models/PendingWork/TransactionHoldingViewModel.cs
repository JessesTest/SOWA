using SW.DM;

namespace SW.InternalWeb.Models.PendingWork;

public class TransactionHoldingViewModel
{
    public TransactionHoldingViewModel() { }

    public TransactionHoldingViewModel(TransactionHolding th)
    {
        AddDateTime = th.AddDateTime;
        AddToi = th.AddToi;
        Approver = th.Approver;
        CheckNumber = th.CheckNumber?.ToString();
        ChgDateTime = th.ChgDateTime;
        ChgToi = th.ChgToi;
        Comment = th.Comment;
        CustomerID = th.CustomerId.ToString();
        CustomerType = th.CustomerType;
        DelDateTime = th.DelDateTime;
        DeleteFlag = th.DeleteFlag;
        DelToi = th.DelToi;
        Sender = th.Sender;
        Status = th.Status;
        TransactionAmt = th.TransactionAmt.ToString("0.00");
        TransactionCode = $"{th.TransactionCode.Code} - {th.TransactionCode.Description}";
        TransactionHoldingId = th.TransactionHoldingId.ToString();
        WorkOrder = th.WorkOrder;
    }

    public string TransactionHoldingId { get; set; }
    public string CustomerType { get; set; }
    public string CustomerID { get; set; }
    public string TransactionCode { get; set; }
    public string TransactionAmt { get; set; }
    public string CheckNumber { get; set; }
    public string Comment { get; set; }
    public string WorkOrder { get; set; }
    public string Status { get; set; }
    public string Sender { get; set; }
    public string Approver { get; set; }
    public bool DeleteFlag { get; set; }
    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
    public DateTime? AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }
}
