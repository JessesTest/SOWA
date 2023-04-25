using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionHoldingService : ITransactionHoldingService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public TransactionHoldingService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<TransactionHolding> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionHoldings
            .Where(c => c.TransactionHoldingId == id)
            .Include(c => c.TransactionCode)
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<TransactionHolding>> GetAllAwaitingPaymentByCustomerId(int customerId, bool includeDeleted)
    {
        using var db = dbFactory.CreateDbContext();
        var customer = await db.GetCustomerById(customerId);

        IQueryable<TransactionHolding> query = db.TransactionHoldings
            .Where(e => e.CustomerId == customer.CustomerId && e.Status == "Awaiting Payment")
            .Include(e => e.TransactionCode);

        if (!includeDeleted)
            query = query.Where(e => !e.DeleteFlag);

        return await query
            .OrderByDescending(e => e.AddDateTime)
            .AsNoTracking()
            .ToListAsync();
    }
}
