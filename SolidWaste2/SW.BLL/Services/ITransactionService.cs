using SW.BLL.DTOs;
using SW.DM;

namespace SW.BLL.Services;

public interface ITransactionService
{
    // Transaction
    Task<Transaction> GetLatesetTransaction(int customerId);

    // Delinquency
    Task<decimal> GetRemainingBalanceFromLastBill(int customerId);
    Task<decimal> GetPastDueAmount(int customerId);
    Task<decimal> Get30DaysPastDueAmount(int customerId);
    Task<decimal> Get60DaysPastDueAmount(int customerId);
    Task<decimal> Get90DaysPastDueAmount(int customerId);
    Task<decimal> GetRemainingCurrentBalance (DateTime date, int days, int customerId);
    Task<decimal> GetCollectionsBalance(int customerId);
    Task<decimal> GetCounselorsBalance(int customerId);
    Task<ICollection<CustomerDelinquency>> GetAllDelinquencies();
    Task<ICollection<Transaction>> GetPayments(DateTime thruDate, Transaction bill);
}
