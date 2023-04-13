using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SW.BLL.DTOs;
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
            .FirstOrDefaultAsync();
    }

    public async Task Update(WorkOrderLegacy workOrderLegacy)
    {
        workOrderLegacy.ChgDateTime = DateTime.Now;

        using var db = dbFactory.CreateDbContext();
        db.WorkOrderLegacies.Update(workOrderLegacy);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<WorkOrderLegacy>> GetInquiryResultList(int? equipmentNumber, string route, DateTime? transDate, string driver, string breakdownLocation, int? problemNumber, bool include)
    {
        using var db = dbFactory.CreateDbContext();

         IQueryable<WorkOrderLegacy> query = db.WorkOrderLegacies
            .Where(e => !e.DelFlag)
            .AsNoTracking();

        if (equipmentNumber.HasValue)
            query = query.Where(e => e.EquipmentNumber == equipmentNumber);

        if (!string.IsNullOrWhiteSpace(route))
            query = query.Where(e => e.EquipmentNumber == equipmentNumber);

        if (transDate.HasValue)
            query = query.Where(e => e.TransDate == transDate);

        if (!string.IsNullOrWhiteSpace(driver))
            query = query.Where(e => e.Driver == driver);

        if (!string.IsNullOrWhiteSpace(breakdownLocation))
            query = query.Where(e => e.BreakdownLocation.Contains(breakdownLocation));

        if (problemNumber.HasValue)
            query = query.Where(e => e.ProblemNumber == problemNumber);

        //if (include)        //this is checked so show closed work orders   (yes   resolved date)
        //    query = query.Where(e => e.ResolveDate.HasValue);

        //if (!include)       //this is unchecked so show open work orders   (no   resolved date)
        //    query = query.Where(e => !e.ResolveDate.HasValue);

        return await query
            .Take(500)
            .ToListAsync();
    }
}
