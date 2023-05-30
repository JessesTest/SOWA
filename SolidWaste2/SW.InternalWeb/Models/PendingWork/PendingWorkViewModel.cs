using SW.DM;

namespace SW.InternalWeb.Models.PendingWork;

public class PendingWorkViewModel
{
    public PendingWorkViewModel() { }

    public PendingWorkViewModel(IEnumerable<TransactionHolding> list)
    {
        TransactionHoldings = list == null ?
            new List<TransactionHoldingViewModel>() :
            list.Select(h => new TransactionHoldingViewModel(h)).ToList();
    }

    public IList<TransactionHoldingViewModel> TransactionHoldings { get; set; }
}
