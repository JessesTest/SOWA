using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;

namespace PE.BL.Services;

public class PersonEntityService : IPersonEntityService
{
    private readonly IDbContextFactory<PeDbContext> contextFactory;

    public PersonEntityService(IDbContextFactory<PeDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task Add(PersonEntity personEntity)
    {
        personEntity.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.People.Add(personEntity);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<PersonEntity>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<PersonEntity> query = db.People;

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PersonEntity> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.People
            .Where(e => e.Id == id)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task Remove(PersonEntity personEntity)
    {
        personEntity.DelDateTime = DateTime.Now;
        personEntity.Delete = true;

        using var db = contextFactory.CreateDbContext();
        db.People.Update(personEntity);
        await db.SaveChangesAsync();
    }

    public async Task Update(PersonEntity personEntity)
    {
        personEntity.ChgDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.People.Update(personEntity);
        await db.SaveChangesAsync();
    }

    public async Task<PersonEntity> GetBySystemAndCode(string system, int code)
    {
        using var db = contextFactory.CreateDbContext();

        var codeString = code.ToString();

        return await db.People
            .Where(p => p.Code.Code1 == system)
            .Where(p => p.Account == codeString)
            .Where(p => !p.Delete)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task SetDefaultAddress(int personEntityId, int addressId)
    {
        using var db = contextFactory.CreateDbContext();
        var address = await db.Addresses
            .Where(e => e.Id == addressId && !e.Delete)
            .SingleOrDefaultAsync();

        if (address == null)
            throw new ArgumentException("Invalid address", nameof(addressId));

        var person = await db.People
            .Where(e => e.Id == personEntityId && !e.Delete)
            .SingleOrDefaultAsync();

        if (person == null)
            throw new ArgumentException("Invalid person", nameof(personEntityId));

        var oldDefaults = await db.Addresses
            .Where(e => e.PersonEntityID == personEntityId && e.IsDefault)
            .ToListAsync();
        foreach (var addr in oldDefaults)
        {
            addr.IsDefault = false;
        }

        address.IsDefault = true;
        await db.SaveChangesAsync();
    }

    public async Task SetDefaultEmail(int personEntityId, int emailId)
    {
        using var db = contextFactory.CreateDbContext();
        var email = await db.Emails
            .Where(e => e.Id == emailId && !e.Delete)
            .SingleOrDefaultAsync();

        if (email == null)
            throw new ArgumentException("Invalid email", nameof(emailId));

        var person = await db.People
            .Where(e => e.Id == personEntityId && !e.Delete)
            .SingleOrDefaultAsync();

        if (person == null)
            throw new ArgumentException("Invalid person", nameof(personEntityId));

        var oldDefaults = await db.Emails
            .Where(e => e.PersonEntityID == personEntityId && e.IsDefault)
            .ToListAsync();
        foreach (var oldEmail in oldDefaults)
        {
            oldEmail.IsDefault = false;
        }

        email.IsDefault = true;
        await db.SaveChangesAsync();
    }

    public async Task SetDefaultPhone(int personEntityId, int phoneId)
    {
        using var db = contextFactory.CreateDbContext();
        var phone = await db.Phones
            .Where(e => e.Id == phoneId && !e.Delete)
            .SingleOrDefaultAsync();

        if (phone == null)
            throw new ArgumentException("Invalid phone", nameof(phoneId));

        var person = await db.People
            .Where(e => e.Id == personEntityId && !e.Delete)
            .SingleOrDefaultAsync();

        if (person == null)
            throw new ArgumentException("Invalid person", nameof(personEntityId));

        var oldDefaults = await db.Phones
            .Where(e => e.PersonEntityID == personEntityId && e.IsDefault)
            .ToListAsync();
        foreach (var oldPhone in oldDefaults)
        {
            oldPhone.IsDefault = false;
        }

        phone.IsDefault = true;
        await db.SaveChangesAsync();
    }
}
