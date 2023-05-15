using SW.DM;

namespace SW.BLL.Services;

public interface ITransactionHoldingService
{
    Task<TransactionHolding> GetById(int id);
    Task<ICollection<TransactionHolding>> GetAllAwaitingPaymentByCustomerId(int customerId, bool includeDeleted);
}
