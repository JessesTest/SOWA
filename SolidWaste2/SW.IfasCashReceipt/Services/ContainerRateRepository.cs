using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.IfasCashReceipt.Services;

public sealed class ContainerRateRepository
{
    private readonly SwDbContext db;

    public ContainerRateRepository(IDbContextFactory<SwDbContext> dbContextFactory)
    {
        db = dbContextFactory.CreateDbContext();
    }

    public Task<List<ContainerRate>> GetContainerRateByCodeDaysSizeEffDate(
        int containerCodeId,
        int containerSubtypeID,
        int dayCount,
        decimal billingSize,
        DateTime effectiveDate)
    {
        return db.ContainerRates
            .Where(c =>
                c.ContainerType == containerCodeId &&
                c.ContainerSubtypeId == containerSubtypeID &&
                c.NumDaysService == dayCount &&
                c.BillingSize == billingSize &&
                c.EffectiveDate <= effectiveDate &&
                !c.DeleteFlag)
            .OrderByDescending(t => t.EffectiveDate)
            .ToListAsync();
    }
}
