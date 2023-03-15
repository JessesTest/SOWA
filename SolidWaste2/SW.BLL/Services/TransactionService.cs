using Microsoft.EntityFrameworkCore;
using SW.BLL.DTOs;
using SW.DAL.Contexts;
using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BLL.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public TransactionService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    #region Utility

    internal async Task<Customer> GetCustomerById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Customers
            .Where(c => c.CustomerId == id || c.LegacyCustomerId == id)
            .AsNoTracking()
            .SingleAsync();
    }

    internal async Task<ICollection<Customer>> GetAllCustomers()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Customers
            .AsNoTracking()
            .ToListAsync();
    }

    #endregion

    #region Transaction

    public async Task<Transaction> GetLatesetTransaction(int customerId)
    {
        using var db = dbFactory.CreateDbContext();

        var customer = await GetCustomerById(customerId);

        return await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId && !t.DeleteFlag)
            .OrderByDescending(t => t.AddDateTime).ThenByDescending(t => t.Sequence).ThenByDescending(t => t.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Delinquency

    public async Task<decimal> GetRemainingBalanceFromLastBill(int customerId)
    {
        return await GetRemainingCurrentBalance(0, customerId);
    }

    internal async Task<decimal> GetPastDueAmount(int daysPastDue, int customerId)
    {
        return await GetRemainingCurrentBalance(daysPastDue + 14, customerId);
    }

    public async Task<decimal> GetPastDueAmount(int customerId)
    {
        return await GetPastDueAmount(1, customerId);
    }

    public async Task<decimal> Get30DaysPastDueAmount(int customerId)
    {

        return await GetPastDueAmount(30, customerId);
    }

    public async Task<decimal> Get60DaysPastDueAmount(int customerId)
    {

        return await GetPastDueAmount(60, customerId);
    }

    public async Task<decimal> Get90DaysPastDueAmount(int customerId)
    {

        return await GetPastDueAmount(90, customerId);
    }

    internal async Task<decimal> GetRemainingCurrentBalance(int days, int customerId)
    {
        return await GetRemainingCurrentBalance(DateTime.Now, days, customerId);
    }

    public async Task<decimal> GetRemainingCurrentBalance(DateTime date, int days, int customerId)
    {
        using var db = dbFactory.CreateDbContext();

        var endDate = date.Date.AddDays(1);
        var startDate = date.Date.AddDays(-days);
        Customer customer = await GetCustomerById(customerId);
        string[] billTypes = { "MB", "FB", "MBR" };

        var bill = await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId
                && !t.DeleteFlag
                && billTypes.Contains(t.TransactionCode.Code)
                && t.AddDateTime < startDate)
            .OrderByDescending(t => t.AddDateTime).ThenByDescending(t => t.Sequence).ThenByDescending(t => t.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (bill == null || bill.TransactionBalance <= 0)
            return 0;

        var transactions = await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId
                && !t.DeleteFlag
                && t.AddDateTime < endDate
                && t.TransactionAmt < 0
                && ((t.AddDateTime > bill.AddDateTime) || (t.AddDateTime == bill.AddDateTime && t.Sequence > bill.Sequence)))
            .AsNoTracking()
            .ToListAsync();

        var total = bill.TransactionBalance + transactions.Sum(t => t.TransactionAmt);
        if (total < 0)
            return 0;

        return total;
    }

    public async Task<decimal> GetCollectionsBalance(int customerId)
    {
        var t = await GetLatesetTransaction(customerId);
        return t != null ? t.CollectionsBalance : 0;
    }

    public async Task<decimal> GetCounselorsBalance(int customerId)
    {
        var t = await GetLatesetTransaction(customerId);
        return t != null ? t.CounselorsBalance : 0;
    }

    public async Task<ICollection<CustomerDelinquency>> GetAllDelinquencies()
    {
        var list = new List<CustomerDelinquency>();
        var customers = await GetAllCustomers();

        foreach (var c in customers)
        {
            var d = new CustomerDelinquency();
            d.Customer = c;
            d.CollectionsBalance = await GetCollectionsBalance(c.CustomerId);
            d.CounselorsBalance = await GetCounselorsBalance(c.CustomerId);

            var d1 = await GetPastDueAmount(c.CustomerId);

            if (d1 > 0)
            {
                var d30 = await Get30DaysPastDueAmount(c.CustomerId);
                var d60 = await Get60DaysPastDueAmount(c.CustomerId);
                var d90 = await Get90DaysPastDueAmount(c.CustomerId);

                d.PastDue = d1 - d30;
                if (d.PastDue < 0)
                    d.PastDue = 0;

                d.PastDue30Days = d30 - d60;
                if (d.PastDue30Days < 0)
                    d.PastDue30Days = 0;

                d.PastDue60Days = d60 - d90;
                if (d.PastDue60Days < 0)
                    d.PastDue60Days = 0;

                d.PastDue90Days = d90;
                if (d.PastDue90Days < 0)
                    d.PastDue90Days = 0;
            }

            // check & service call should be moved to controller where this method is being called from?
            //var pebl = new PE.BL.BusinessLayer();
            //if (d.IsDelinquent)
            //{
            //    d.PersonEntity = pebl.GetPersonEntityById(c.PE);
            //    list.Add(d);
            //}
        }

        return list;
    }

    public async Task<ICollection<Transaction>> GetPayments(DateTime thruDate, Transaction bill)
    {
        using var db = dbFactory.CreateDbContext();

        var date = thruDate.Date.AddDays(1);
        return await db.Transactions
            .Where(t => !t.DeleteFlag
                && t.TransactionAmt < 0
                && t.CustomerId == bill.CustomerId
                && t.AddDateTime < date
                && ((t.AddDateTime > bill.AddDateTime) || (t.AddDateTime == bill.AddDateTime && t.Sequence > bill.Sequence)))
            .AsNoTracking()
            .ToListAsync();
    }

    #endregion
}
