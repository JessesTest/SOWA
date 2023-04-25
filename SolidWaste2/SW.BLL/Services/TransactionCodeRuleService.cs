using Microsoft.EntityFrameworkCore;
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
            .Where(e => e.TransactionCodeId == transactionCodeId && !e.DeleteFlag)
            .Include(e => e.Formula).ThenInclude(f => f.Parameters)
            .Include(e => e.TransactionCode)
            .Include(e => e.ContainerCode)
            .Include(e => e.ContainerSubtype)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<ICollection<TransactionCodeRule>> GetByContainerAndTransactionCode(Container container, int transactionCodeId)
    {
        using var db = dbFactory.CreateDbContext();

        return await db.TransactionCodeRules
            .Where(e => !e.DeleteFlag 
                && e.TransactionCodeId == transactionCodeId 
                && e.ContainerCodeId == container.ContainerCodeId
                && e.ContainerSubtypeId == container.ContainerSubtypeId
                && e.ContainerNumDaysService == container.NumDaysService
                && e.ContainerBillingSize == container.BillingSize)
            .Include(e => e.Formula).ThenInclude(f => f.Parameters)
            .Include(e => e.TransactionCode)
            .Include(e => e.ContainerCode)
            .Include(e => e.ContainerSubtype)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
}
