using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class BillMasterService : IBillMasterService
{
    private readonly IDbContextFactory<SwDbContext> contextFactory;

    public BillMasterService(IDbContextFactory<SwDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<BillMaster> GetByTransaction(int transactionId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.BillMasters
            .Where(e => e.TransactionId == transactionId)
            .Include(e => e.BillServiceAddresses)
            .ThenInclude(e => e.BillContainerDetails)
            .AsSplitQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<BillMaster> GetMostRecentBillMaster(int customerId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.BillMasters
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.AddDateTime)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<BillMaster> GetPreviousBillMaster(int customerId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.BillMasters
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.AddDateTime)
            .Skip(1)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
