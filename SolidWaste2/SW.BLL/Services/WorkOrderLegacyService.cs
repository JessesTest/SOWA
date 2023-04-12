using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class WorkOrderLegacyService : IWorkOrderLegacyService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public WorkOrderLegacyService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<WorkOrderLegacy> GetById(int workOrderLegacyId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.WorkOrderLegacies
            .Where(e => e.WorkOrderLegacyId == workOrderLegacyId)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task Update(WorkOrderLegacy workOrderLegacy)
    {
        workOrderLegacy.ChgDateTime = DateTime.Now;

        using var db = dbFactory.CreateDbContext();
        db.WorkOrderLegacies.Update(workOrderLegacy);
        await db.SaveChangesAsync();
    }
}
