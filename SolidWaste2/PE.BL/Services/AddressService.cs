using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;

namespace PE.BL.Services;

public class AddressService : IAddressService
{
    private readonly IDbContextFactory<PeDbContext> contextFactory;

    public AddressService(IDbContextFactory<PeDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task Add(Address address)
    {
        address.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Addresses.Add(address);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Address>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Addresses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Address> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Addresses
            .Where(e => e.Id == id)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<Address>> GetByPerson(int personId, bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Address> query = db.Addresses.Where(e => e.PersonEntityID == personId);
        if (!includeDeleted)
            query = query.Where(e => e!.Delete);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Remove(Address address)
    {
        address.DelDateTime = DateTime.Now;
        address.Delete = true;

        using var db = contextFactory.CreateDbContext();
        db.Addresses.Update(address);
        await db.SaveChangesAsync();
    }

    public async Task Update(Address address)
    {
        address.ChgDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Addresses.Update(address);
        await db.SaveChangesAsync();
    }
}
