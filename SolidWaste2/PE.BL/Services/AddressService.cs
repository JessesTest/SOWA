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
        address.Delete = false;
        address.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.Addresses.Add(address);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Address>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Address> query = db.Addresses
            .Include(e => e.Code);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Address> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Addresses
            .Where(e => e.Id == id)
            .Include(e => e.Code)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<Address>> GetByPerson(int personId, bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<Address> query = db.Addresses
            .Where(e => e.PersonEntityID == personId)
            .Include(e => e.Code);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

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

    public async Task SetDefault(int personId, int addressId)
    {
        using var db = contextFactory.CreateDbContext();

        var defaultAddress = await GetById(addressId);

        if (defaultAddress == null)
            throw new Exception(string.Format("Address Id '{0}' was not found.", addressId));
        if (defaultAddress.PersonEntityID != personId)
            throw new Exception("Address PersonEntityId mismatch");
        if (defaultAddress.Delete)
            throw new Exception("Address was deleted");

        var defaultAddresses = await db.Addresses
            .Where(a => a.PersonEntityID == personId && a.IsDefault)
            .ToListAsync();

        foreach (var address in defaultAddresses)
        {
            address.IsDefault = false;
            await Update(address);
        }

        defaultAddress.IsDefault = true;
        await Update(defaultAddress);
    }
}
