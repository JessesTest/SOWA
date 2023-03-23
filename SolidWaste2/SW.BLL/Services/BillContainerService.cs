using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class BillContainerService : IBillContainerService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public BillContainerService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<BillContainerDetail>> GetByBillServiceAddress(int billServiceAddressId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.BillContainerDetails
            .Where(e => e.BillServiceAddressId == billServiceAddressId)
            .AsNoTracking()
            .ToListAsync();
    }
}
