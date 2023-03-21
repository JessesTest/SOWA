using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;

namespace PE.BL.Services;

public class CodeService : ICodeService
{
    private readonly IDbContextFactory<PeDbContext> contextFactory;

    public CodeService(IDbContextFactory<PeDbContext> dbFactory)
    {
        this.contextFactory = dbFactory;
    }

    public async Task Add(Code code)
    {
        code.Delete = false;
        code.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Codes.Add(code);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Code>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Codes
            .Where(e => !e.Delete)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Code> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Codes
            .Where(e => e.Id == id)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<Code>> GetByType(string type, bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Code> query = db.Codes
            .Where(e => e.Type == type);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Remove(Code code)
    {
        code.Delete = true;
        code.DelDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Codes.Update(code);
        await db.SaveChangesAsync();
    }

    public async Task Update(Code code)
    {
        code.ChgDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Codes.Update(code);
        await db.SaveChangesAsync();
    }
}
