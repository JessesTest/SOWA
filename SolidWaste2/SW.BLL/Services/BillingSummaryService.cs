using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;

namespace SW.BLL.Services;

public class BillingSummaryService : IBillingSummaryService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public BillingSummaryService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<decimal> GetTotalPaymentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate)
    {
        string[] paymentCodes = { "CC", "KP", "P", "BW", "PP", "IP", "JE", "ACH" };

        using var db = dbFactory.CreateDbContext();
        return await db.Transactions
            .Where(t => t.CustomerId == customerId)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime >= startDate)
            .Where(t => t.AddDateTime <= endDate)
            .Where(t => paymentCodes.Contains(t.TransactionCode.Code))
            .SumAsync(t => t.TransactionAmt);
    }

    public async Task<decimal> GetTotalAdjustmentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate)
    {
        string[] paymentCodes = { "BD", "R", "C", "DER", "DEM", "RC", "DEP", "DEC", "DEK", "B" };

        using var db = dbFactory.CreateDbContext();
        return await db.Transactions
            .Where(t => t.CustomerId == customerId)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime >= startDate)
            .Where(t => t.AddDateTime <= endDate)
            .Where(t => paymentCodes.Contains(t.TransactionCode.Code))
            .SumAsync(t => t.TransactionAmt);
    }

    public async Task<decimal> GetTotalNewChargesForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate)
    {
        string[] paymentCodes = { "BK", "EP", "MB", "RD", "RR", "MBR", "FB", "LF", "CF", "WC", "WRC", "WRO", "WRR", "CR" };

        using var db = dbFactory.CreateDbContext();
        return await db.Transactions
            .Where(t => t.CustomerId == customerId)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime >= startDate)
            .Where(t => t.AddDateTime <= endDate)
            .Where(t => paymentCodes.Contains(t.TransactionCode.Code))
            .SumAsync(t => t.TransactionAmt);
    }
}
