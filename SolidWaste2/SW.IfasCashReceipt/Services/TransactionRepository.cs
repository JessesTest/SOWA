using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.IfasCashReceipt.Services;

public sealed class TransactionRepository
{
    private readonly SwDbContext db;

    public TransactionRepository(IDbContextFactory<SwDbContext> dbFactory)
    {
        db = dbFactory.CreateDbContext();
    }

    public Task SaveChangesAsync()
    {
        return db.SaveChangesAsync();
    }

    public async Task<List<Transaction>> GetPaymentsByAddDateTimeRange(
        DateTime cash_rcpt_beg_datetime,
        DateTime cash_rcpt_end_datetime)
        //int customer_id
    {
        var codes = new[] { "CC", "P", "BW", "IP", "ACH", "PP", "PV", "PCC" };

        var transactions = await db.Transactions
            .Where(t => codes.Contains(t.TransactionCode.Code))
            //.Where(t => t.CustomerId == customer_id)
            .Where(t => t.AddDateTime >= cash_rcpt_beg_datetime)
            .Where(t => t.AddDateTime < cash_rcpt_end_datetime)
            .Where(t => !t.DeleteFlag)
            .Where(t => (t.Comment == null || !t.Comment.Contains("KanPay")))
            .Include(t => t.Customer)   // this may not add the customer
            .Include(t => t.TransactionCode)
            .OrderBy(t => t.CustomerId)
            .ThenByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .AsSplitQuery()
            .ToListAsync();

        return await AddCustomers(transactions);
    }

    public async Task<List<Transaction>> GetUnpaidChargesForCustomer(Customer customer)
    {
        int customerId = customer.CustomerId;
        var codes = new[] { "MB", "FB", "LF", "CF", "WC", "WRC", "WRO", "WRR", "RF", "RD", "RR", "EP", "BK", "CR" };

        var transactions = await db.Transactions
            .Where(t => codes.Contains(t.TransactionCode.Code))
            .Where(t => !t.DeleteFlag)
            .Where(t => t.PaidFull != true)
            .Where(t => t.CustomerId == customerId)
            .Include(t => t.Customer)   // this may not add the customer
            .Include(t => t.TransactionCode)
            .Include(t => t.Container)
            .ThenInclude(c => c.ContainerCode)
            .OrderBy(t => t.AddDateTime)
            .ThenBy(t => t.Sequence)
            .ThenBy(t => t.Id)
            .ToListAsync();

        foreach(var transaction in transactions)
        {
            if (transaction.Customer != null)
                continue;

            transaction.Customer = customer;
        }

        return transactions;
    }

    public async Task<List<Transaction>> GetUpdatedChargeTransactionsByAddDateTimeRangeForCashReceipt(
        DateTime cash_receipt_beg_datetime,
        DateTime cash_receipt_end_datetime)
    {
        var transactions = await db.Transactions
            .Where(t =>
                t.ChgDateTime >= cash_receipt_beg_datetime &&
                t.ChgDateTime <= cash_receipt_end_datetime &&
                t.ChgToi == "Cash Receipt" &&
                !t.DeleteFlag)
            .Include(t => t.Customer)   // this may not add the customer
            .Include(t => t.TransactionCode)
            .ToListAsync();

        return await AddCustomers(transactions);
    }

    private async Task<List<Transaction>> AddCustomers(List<Transaction> transactions)
    {
        var customerIds = transactions
            .Where(t => t.Customer == null)
            .Select(t => t.CustomerId).Distinct()
            .ToArray();
        var customers = await db.Customers
            .Where(c => customerIds.Contains(c.CustomerId))
            .ToListAsync();

        foreach(var transaction in transactions)
        {
            if (transaction.Customer != null)
                continue;

            var customer = customers.SingleOrDefault(c => 
                c.CustomerId == transaction.CustomerId
                /* && c.CustomerType == transaction.CustomerType */);

            transaction.Customer = customer;
        }

        return transactions;
    }
}
