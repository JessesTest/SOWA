using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionCodeRuleService : ITransactionCodeRuleService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public TransactionCodeRuleService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<TransactionCodeRule>> GetByTransactionCodeId(int transactionCodeId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodeRules
            .Where(e => !e.DeleteFlag
                && e.TransactionCodeId.HasValue
                && e.TransactionCodeId.Value == transactionCodeId)
            .Include(e => e.Formula).ThenInclude(f => f.Parameters)
            .Include(e => e.TransactionCode)
            .Include(e => e.ContainerCode)
            .Include(e => e.ContainerSubtype)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<ICollection<TransactionCodeRule>> GetByContainerAndTransactionCode(int transactionCodeId, Container container = null)
    {
        using var db = dbFactory.CreateDbContext();

        var query = db.TransactionCodeRules
            .Where(e => !e.DeleteFlag
                && (e.TransactionCodeId == null || e.TransactionCodeId.Value == transactionCodeId))
            .Include(e => e.Formula).ThenInclude(f => f.Parameters)
            .Include(e => e.TransactionCode)
            .Include(e => e.ContainerCode)
            .Include(e => e.ContainerSubtype)
            .AsSplitQuery();

        if (container != null)
            query = query.Where(e => (e.ContainerCodeId == null || e.ContainerCodeId.Value == container.ContainerCodeId)
                && (e.ContainerSubtypeId == null || e.ContainerSubtypeId.Value == container.ContainerSubtypeId)
                && (e.ContainerNumDaysService == null || e.ContainerNumDaysService.Value == container.NumDaysService)
                && (e.ContainerBillingSize == null || e.ContainerBillingSize.Value == container.BillingSize));

        return await query
            .AsNoTracking()
            .ToListAsync();
    }
}
