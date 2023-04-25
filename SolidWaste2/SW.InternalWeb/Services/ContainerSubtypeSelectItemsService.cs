using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;

namespace SW.InternalWeb.Services;

public class ContainerSubtypeSelectItemsService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ContainerSubtypeSelectItemsService(
        IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<IEnumerable<SelectListItem>> Get()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerSubtypes
            .Where(s => !s.DeleteFlag)
            .OrderBy(s => s.Description)
            .Select(s => new SelectListItem
            {
                Value = s.ContainerSubtypeId.ToString(),
                Text = $"{s.BillingFrequency} - {s.Description}"
            })
            .ToListAsync();
    }
}
