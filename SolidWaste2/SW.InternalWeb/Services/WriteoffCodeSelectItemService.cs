using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;

namespace SW.InternalWeb.Services;

public class WriteoffCodeSelectItemService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public WriteoffCodeSelectItemService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<IEnumerable<SelectListItem>> Get()
    {
        var codes = new string[] { "UWO", "VWO", "CWO", "BWO" };

        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(tc => codes.Contains(tc.Code) && !tc.DeleteFlag)
            .Select(tc => new SelectListItem { Value = tc.Code, Text = tc.Description })
            .OrderBy(sli => sli.Text)
            .ToListAsync();
    }
}
