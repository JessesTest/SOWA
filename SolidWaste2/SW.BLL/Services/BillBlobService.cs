using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class BillBlobService : IBillBlobService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public BillBlobService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<BillBlobs> GetByTransactionId(int transactionId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.BillBlobs
            .Where(e => e.TransactionId == transactionId && !e.DelFlag)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }
}
