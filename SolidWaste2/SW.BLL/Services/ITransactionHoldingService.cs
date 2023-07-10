using SW.DM;

namespace SW.BLL.Services;

public interface ITransactionHoldingService
{
    Task<ICollection<TransactionHolding>> GetAll();
    Task<ICollection<TransactionHolding>> GetAllAuthorized(string email);
    Task<TransactionHolding> GetAuthorizedById(string email, int id);
    Task<bool> IsAuthorized(int transactionHoldingId, string email);
    Task<TransactionHolding> GetById(int id);
    Task<ICollection<TransactionHolding>> GetAllAwaitingPaymentByCustomerId(int customerId, bool includeDeleted);

    Task Add(TransactionHolding transactionHolding);
    Task Update(TransactionHolding transactionHolding);
    Task Delete(TransactionHolding transactionHolding);

    Task<string> Resolve(TransactionHolding transactionHolding, string email, string displayName);
    Task<string> Approve(TransactionHolding transactionHolding, string email, string displayName);
    Task<string> Reject(TransactionHolding transactionHolding, string email, string displayName);

    // Batch logic
    Task<bool> IsBatchIdValid(int batchId);
    Task<ICollection<TransactionHolding>> GetBatchTransactionsById(int batchId);
    Task<int> GetNextBatchId();
    Task<IDictionary<int, string>> GetActiveBatches();
}
