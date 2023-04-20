using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BLL.Services;

public class RouteTypeService : IRouteTypeService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public RouteTypeService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<RouteType>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.RouteTypes
            .Where(e => !e.DeleteFlag)
            .OrderBy(e => e.Type)
            .AsNoTracking()
            .ToListAsync();
    }
}
