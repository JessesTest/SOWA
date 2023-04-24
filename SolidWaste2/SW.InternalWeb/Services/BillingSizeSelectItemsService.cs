using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;

namespace SW.InternalWeb.Services;

public class BillingSizeSelectItemsService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public BillingSizeSelectItemsService(
        IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }
    public async Task<IEnumerable<SelectListItem>> Get(
        int containerCodeId,
        int containerSubtypeId,
        int daysOfService,
        DateTime effectiveDate)
    {
        var today = DateTime.Today;
        if (today >= effectiveDate)
            effectiveDate = today;

        using var db = dbFactory.CreateDbContext();
        var temp = await db.ContainerRates
            .Where(r =>
                r.ContainerType == containerCodeId &&
                r.ContainerSubtypeId == containerSubtypeId &&
                r.NumDaysService == daysOfService &&
                !r.DeleteFlag &&
                r.EffectiveDate <= effectiveDate)
            .Select(r => new { r.BillingSize, r.RateDescription })
            .Distinct()
            .OrderBy(r => r.BillingSize)
            .ToListAsync();

        if (!temp.Any())
            return new[] { new SelectListItem{ Value = "0.0", Text = "No Rates Found" } };

        return temp
            .Select(r => new SelectListItem { Value = $"{r.BillingSize}", Text = $"{r.BillingSize:0.0} - {r.RateDescription}" })
            .ToList();
    }
}
