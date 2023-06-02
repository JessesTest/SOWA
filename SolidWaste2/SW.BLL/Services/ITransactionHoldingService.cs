using SW.DM;

namespace SW.BLL.Services;

public interface ITransactionHoldingService
{
    Task<ICollection<TransactionHolding>> GetAll();
    Task<ICollection<TransactionHolding>> GetAllAuthorized(string email);
    Task<TransactionHolding> GetAuthorizedById(string email, int id);
    Task<TransactionHolding> GetById(int id);
    Task<ICollection<TransactionHolding>> GetAllAwaitingPaymentByCustomerId(int customerId, bool includeDeleted);


    Task<string> Resolve(int transactionHoldingId, string email, string displayName);
    Task<string> Approve(int transactionHoldingId, string email, string displayName);
    Task<string> Reject(int transactionHoldingId, string comment, string email, string displayName);
}
