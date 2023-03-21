using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;

namespace PE.BL.Services;

public class PhoneService : IPhoneService
{
    private readonly IDbContextFactory<PeDbContext> contextFactory;

    public PhoneService(IDbContextFactory<PeDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task Add(Phone phone)
    {
        phone.Delete = false;
        phone.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Phones.Add(phone);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Phone>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Phone> query = db.Phones
            .Include(e => e.Code);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Phone> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Phones
            .Where(e => e.Id == id)
            .Include(e => e.Code)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<Phone>> GetByPerson(int personId, bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Phone> query = db.Phones
            .Where(e => e.PersonEntityID == personId)
            .Include(e => e.Code);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Remove(Phone phone)
    {
        phone.DelDateTime = DateTime.Now;
        phone.Delete = true;

        using var db = contextFactory.CreateDbContext();
        db.Phones.Update(phone);
        await db.SaveChangesAsync();
    }

    public async Task Update(Phone phone)
    {
        phone.ChgDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Phones.Update(phone);
        await db.SaveChangesAsync();
    }

    public async Task SetDefault(int personId, int phoneId)
    {
        using var db = contextFactory.CreateDbContext();

        var defaultPhone = await GetById(phoneId);

        if (defaultPhone == null)
            throw new Exception(string.Format("Phone Id '{0}' was not found.", phoneId));
        if (defaultPhone.PersonEntityID != personId)
            throw new Exception("Phone PersonEntityId mismatch");
        if (defaultPhone.Delete)
            throw new Exception("Phone was deleted");

        var defaultPhones = await db.Phones
            .Where(p => p.PersonEntityID == personId && p.IsDefault)
            .ToListAsync();

        foreach(var phone in defaultPhones)
        {
            phone.IsDefault = false;
            await Update(phone);
        }

        defaultPhone.IsDefault = true;
        await Update(defaultPhone);
    }
}
