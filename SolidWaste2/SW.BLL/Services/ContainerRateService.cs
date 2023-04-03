using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class ContainerRateService : IContainerRateService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ContainerRateService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<ContainerRate>> GetContainerRateByCodeDaysSize(
        int containerSubtypeId,
        int dayCount,
        decimal billingSize,
        DateTime effective_date)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(c => c.ContainerSubtypeId == containerSubtypeId
            && c.NumDaysService == dayCount
            && c.BillingSize == billingSize
            && !c.DeleteFlag
            && c.EffectiveDate <= effective_date)
            .ToListAsync();
    }

    public async Task<ICollection<int>> GetDaysOfServiceByContainerSubtype(int containerSubtypeId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(e => !e.DeleteFlag && e.ContainerSubtypeId == containerSubtypeId && e.NumDaysService != null)
            .Select(e => e.NumDaysService.Value)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();
    }
}
