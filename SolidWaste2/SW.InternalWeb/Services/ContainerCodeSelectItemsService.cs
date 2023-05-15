using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;

namespace SW.InternalWeb.Services;

public class ContainerCodeSelectItemsService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ContainerCodeSelectItemsService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<IEnumerable<SelectListItem>> Get()
    {
        using var db = dbFactory.CreateDbContext();
        var temp = await db.ContainerCodes
            .Where(c => !c.DeleteFlag)
            .OrderBy(c => c.Type)
            .Select(c => new { c.ContainerCodeId, c.Type, c.Description })
            .ToListAsync();

        return temp
            .Select(c => new SelectListItem
            {
                Value = $"{c.ContainerCodeId}",
                Text = $"{c.Type} - {c.Description}"
            })
            .ToList();
    }
}
