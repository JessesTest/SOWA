using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionCodeService : ITransactionCodeService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public TransactionCodeService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<TransactionCode>> CollectionPaymentCodes()
    {
        var codes = new[] { "PV", "PCC", "PU", "V2U", "C2U" };
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => codes.Contains(c.Code))
            .OrderBy(c => c.Description)
            .ToListAsync();
    }

    public async Task<TransactionCode> GetByCode(string code)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => c.Code == code && !c.DeleteFlag)
            .SingleOrDefaultAsync();
    }
}
