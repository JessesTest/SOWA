using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class ContainerCodeService : IContainerCodeService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ContainerCodeService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ContainerCode> GetById(int containerCodeId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerCodes
            .Where(e => e.ContainerCodeId == containerCodeId)
            .SingleOrDefaultAsync();
    }
}
