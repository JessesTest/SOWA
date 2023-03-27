using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;
using System.Text;

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
        if (!personEntity.NameTypeFlag)
        {
            if (!string.IsNullOrWhiteSpace(personEntity.FullName))
            {
                string[] parsedName = ParseName(personEntity.FullName);
                personEntity.FirstName = parsedName[0];
                personEntity.MiddleName = parsedName[1];
                personEntity.LastName = parsedName[2];
            }
            else
            {
                personEntity.FullName = FormatName(personEntity.FirstName, personEntity.MiddleName, personEntity.LastName);
            }
        }

        personEntity.Account = await GetNextAccountNumber();
        personEntity.WhenCreated = DateTime.Now;
        personEntity.Delete = false;
        personEntity.AddDateTime = DateTime.Now;

        using var db = contextFactory.CreateDbContext();
        db.People.Add(personEntity);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<PersonEntity>> GetAll(bool includeDeleted)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<PersonEntity> query = db.People
            .Include(e => e.Code)
            .Include(e => e.Addresses)
            .Include(e => e.Emails)
            .Include(e => e.Phones);

        if (!includeDeleted)
            query = query.Where(e => !e.Delete);

        return await query
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<PersonEntity> GetById(int id)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.People
            .Where(e => e.Id == id)
            .Include(e => e.Code)
            .Include(e => e.Addresses)
            .Include(e => e.Emails)
            .Include(e => e.Phones)
            .AsSplitQuery()
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
        if (!personEntity.NameTypeFlag)
        {
            if (!string.IsNullOrWhiteSpace(personEntity.FullName))
            {
                string[] parsedName = ParseName(personEntity.FullName);
                personEntity.FirstName = parsedName[0];
                personEntity.MiddleName = parsedName[1];
                personEntity.LastName = parsedName[2];
            }
            else
            {
                personEntity.FullName = FormatName(personEntity.FirstName, personEntity.MiddleName, personEntity.LastName);
            }
        }

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

        if (address.PersonEntityID != personEntityId)
            throw new ArgumentException("Address PersonEntityID mismatch", nameof(addressId));

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

        if(email.PersonEntityID != personEntityId)
            throw new ArgumentException("Email PersonEntityID mismatch", nameof(emailId));

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

        if (phone.PersonEntityID != personEntityId)
            throw new ArgumentException("Phone PersonEntityID mismatch", nameof(phoneId));

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

    public async Task<string> GetNextAccountNumber()
    {
        using var db = contextFactory.CreateDbContext();

        var seed = await db.People
            .OrderByDescending(e => e.Id)
            .Select(e => e.Account)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = "0";
        }

        Random rand = new Random();

        int increment = rand.Next(1, 20);

        return (int.Parse(seed) + increment).ToString();
    }

    #region Utility

    private static string[] ParseName(string name)
    {
        string[] parsedName = new string[3];
        string[] splitComma = name.Split(',');

        // If name format is [Last, First Middle]
        if (splitComma.Length > 1)
        {
            parsedName[2] = splitComma[0];
            string[] splitName = splitComma[1].Trim().Split(' ');

            switch (splitName.Length)
            {
                case 0:
                    break;
                case 1:
                    parsedName[0] = splitName[0];
                    parsedName[1] = string.Empty;
                    break;
                case 2:
                    parsedName[0] = splitName[0];
                    parsedName[1] = splitName[1];
                    break;
                default:
                    parsedName[0] = splitName[0];
                    parsedName[1] = splitName[1];
                    for (int i = 2; i < splitName.Length; i++)
                    {
                        parsedName[1] += splitName[i];
                    }
                    break;
            }
        }
        // If name format is [First Middle Last]
        else if (splitComma.Length == 1)
        {
            string[] splitName = name.Split(' ');

            switch (splitName.Length)
            {
                case 0:
                    break;
                case 1:
                    parsedName[0] = splitName[0];
                    parsedName[1] = string.Empty;
                    parsedName[2] = string.Empty;
                    break;
                case 2:
                    parsedName[0] = splitName[0];
                    parsedName[1] = string.Empty;
                    parsedName[2] = splitName[1];
                    break;
                case 3:
                    parsedName[0] = splitName[0];
                    parsedName[1] = splitName[1];
                    parsedName[2] = splitName[2];
                    break;
                default:
                    parsedName[0] = splitName[0];
                    parsedName[1] = splitName[1];
                    parsedName[2] = splitName[2];
                    for (int i = 3; i < splitName.Length; i++)
                    {
                        parsedName[2] += splitName[i];
                    }
                    break;
            }
        }

        return parsedName;
    }

    private static string FormatName(params string[] strParams)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < strParams.Length; i++)
        {
            if (i != 0 && !string.IsNullOrEmpty(strParams[i]))
            {
                sb.Append(" ");
            }
            sb.Append(strParams[i]);
        }

        return sb.ToString();
    }

    #endregion
}
