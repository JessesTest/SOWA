using SW.DM;

namespace SW.BLL.Services
{
    public interface ITransactionService
    {
        Task AddTransaction(Transaction transaction);
        Task<ICollection<Transaction>> GetByCustomer(int customerId);
        Task<decimal> GetCurrentBalance(int customerId);
        Task<Transaction> GetLatest(int customerId);
        Task<Transaction> GetById(int transactionId);
        Task AddKanpayTransaction(Transaction transaction, TransactionKanPayFee fee, int kanpayid, string user);
    }
}