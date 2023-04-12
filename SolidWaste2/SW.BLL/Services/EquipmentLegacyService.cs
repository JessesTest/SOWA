using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class EquipmentLegacyService : IEquipmentLegacyService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public EquipmentLegacyService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<EquipmentLegacy>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.EquipmentLegacies
            .Where(e => !e.DelFlag)
            .OrderBy(e => e.EquipmentNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EquipmentLegacy> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.EquipmentLegacies
            .Where(e => e.EquipmentLegacyId == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<EquipmentLegacy> GetByEquipmentNumber(int equipmentNumber)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.EquipmentLegacies
            .Where(e => e.EquipmentNumber == equipmentNumber && !e.DelFlag)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
