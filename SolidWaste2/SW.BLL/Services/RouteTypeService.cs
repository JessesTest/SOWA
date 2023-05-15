using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class RouteTypeService : IRouteTypeService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public RouteTypeService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task Add(RouteType type)
    {
        using var db = dbFactory.CreateDbContext();
        db.RouteTypes.Add(type);
        await db.SaveChangesAsync();
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

    public async Task<RouteType> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.RouteTypes
            .FindAsync(id);
    }

    public async Task<RouteType> GetByRouteNumber(int routeNumber)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.RouteTypes
            .Where(t => t.RouteNumber == routeNumber && !t.DeleteFlag)
            .FirstOrDefaultAsync();
    }

    public async Task Update(RouteType type)
    {
        using var db = dbFactory.CreateDbContext();
        db.RouteTypes.Update(type);
        await db.SaveChangesAsync();
    }
}
