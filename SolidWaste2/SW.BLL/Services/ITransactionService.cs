using SW.BLL.DTOs;
using SW.DM;

namespace SW.BLL.Services;

public interface ITransactionService
{
    // Transaction
    Task AddTransaction(Transaction transaction);
    Task<ICollection<Transaction>> GetByCustomer(int customerId, bool includeDeleted);
    Task<decimal> GetCurrentBalance(int customerId);
    Task<Transaction> GetLatest(int customerId);
    Task<Transaction> GetById(int transactionId);
    Task<ICollection<Transaction>> GetAllUnpaidLateFeesByCustomerId(int customerId);
    Task<ICollection<TransactionListingItem>> GetListingByCustomer(int customerId, bool includeDeleted = false);

    // Delinquency 
    Task<decimal> GetRemainingBalanceFromLastBill(int customerId);
    Task<decimal> GetPastDueAmount(int customerId);
    Task<decimal> Get30DaysPastDueAmount(int customerId);
    Task<decimal> Get60DaysPastDueAmount(int customerId);
    Task<decimal> Get90DaysPastDueAmount(int customerId);
    Task<decimal> GetRemainingCurrentBalance(DateTime date, int days, int customerId);
    Task<decimal> GetCollectionsBalance(int customerId);
    Task<decimal> GetCounselorsBalance(int customerId);
    Task<ICollection<CustomerDelinquency>> GetAllDelinquencies();
    Task<ICollection<Transaction>> GetPayments(DateTime thruDate, Transaction bill);
    Task<ICollection<Transaction>> GetLatestTransactionsWithDelinquency();
    Task MakeDelinquencyPayment(int customerId, string transactionTypeCode, decimal amount, string comment, DateTime? dateTime = null);

    // KanPay
    Task AddKanpayTransaction(Transaction transaction, TransactionKanPayFee fee, int kanpayid, string user);


    Task<DateTime> GetLastBillTranDateTime(int customerID, DateTime billAddDateTime);
}
