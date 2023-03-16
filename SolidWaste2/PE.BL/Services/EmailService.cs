using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;

namespace PE.BL.Services;

public class EmailService : IEmailService
{
    private readonly IDbContextFactory<PeDbContext> contextFactory;

    public EmailService(IDbContextFactory<PeDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task Add(Email email)
    {
        email.Delete = false;
        email.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Emails.Add(email);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Email>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Email> query = db.Emails
            .Include(e => e.Code);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query.AsNoTracking().ToListAsync();

    }

    public async Task<Email> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Emails
            .Where(e => e.Id == id)
            .Include(e => e.Code)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<Email>> GetByPerson(int personId, bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Email> query = db.Emails
            .Where(e => e.PersonEntityID == personId)
            .Include(e => e.Code);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Remove(Email email)
    {
        email.Delete = true;
        email.DelDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Emails.Update(email);
        await db.SaveChangesAsync();
    }

    public async Task Update(Email email)
    {
        email.ChgDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Emails.Update(email);
        await db.SaveChangesAsync();
    }
}
