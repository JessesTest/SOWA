using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class MieDataService : IMieDataService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public MieDataService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<MieData> GetById(int mieDataId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.MieDatas.FindAsync(mieDataId);
    }

    public async Task<ICollection<MieDataInfo>> Get(string miedataImageID, bool miedataActive)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.MieDatas
            .Where(i => i.MieDataImageId == miedataImageID && i.MieDataActive == miedataActive && !i.MieDataDelete)
            .Select(i => new MieDataInfo
            {
                AddDateTime = i.AddDateTime,
                AddToi = i.AddToi,
                ChgDateTime = i.ChgDateTime,
                ChgToi = i.ChgToi,
                DelDateTime = i.DelDateTime,
                DelToi = i.DelToi,
                MieDataActive = i.MieDataActive,
                MieDataDelete = i.MieDataDelete,
                MieDataId = i.MieDataId,
                MieDataImageFileName = i.MieDataImageFileName,
                MieDataImageId = i.MieDataImageId,
                MieDataImageSize = i.MieDataImageSize,
                MieDataImageType = i.MieDataImageType
            })
            .ToListAsync();
    }

    public async Task Add(MieData mieData)
    {
        _ = mieData ?? throw new ArgumentNullException(nameof(mieData));
        mieData.AddToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.MieDatas.Add(mieData);
        await db.SaveChangesAsync();
    }

    public async Task Update(MieData mieData)
    {
        _ = mieData ?? throw new ArgumentNullException(nameof(mieData));

        mieData.ChgDateTime ??= DateTime.Now;
        mieData.ChgToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.MieDatas.Update(mieData);
        await db.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        using var db = dbFactory.CreateDbContext();
        var mieData = await db.MieDatas.FindAsync(id);
        if (mieData == null || mieData.MieDataDelete)
            return;

        mieData.MieDataDelete = true;
        mieData.DelDateTime ??= DateTime.Now;
        mieData.DelToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        db.MieDatas.Update(mieData);
        await db.SaveChangesAsync();
    }
}
