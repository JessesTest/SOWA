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
            .Include(e => e.Addresses.Where(a => !a.Delete))
            .ThenInclude(a => a.Code)
            .Include(e => e.Emails.Where(e => !e.Delete))
            .ThenInclude(e => e.Code)
            .Include(e => e.Phones.Where(p => !p.Delete))
            .ThenInclude(p => p.Code)
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


        _ = await db.People
            .Where(e => e.Id == personEntityId && !e.Delete)
            .SingleOrDefaultAsync()
            ?? throw new ArgumentException("Invalid person", nameof(personEntityId));

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

        _ = await db.People
            .Where(e => e.Id == personEntityId && !e.Delete)
            .SingleOrDefaultAsync()
            ?? throw new ArgumentException("Invalid person", nameof(personEntityId));

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

        _ = await db.People
            .Where(e => e.Id == personEntityId && !e.Delete)
            .SingleOrDefaultAsync()
            ?? throw new ArgumentException("Invalid person", nameof(personEntityId));

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

    #region Search

    public async Task<ICollection<PersonEntity>> Search(
        string fullName,
        string firstName,
        string middleName,
        string lastName,
        string billingAddress,
        string serviceAddress,
        string departmentCodeCode,
        string pin = null,
        IEnumerable<int> personEntityIds = null)
    {
        using var db = contextFactory.CreateDbContext();
        IQueryable<PersonEntity> query = db.People
            .Where(p => !p.Delete);

        if (!string.IsNullOrWhiteSpace(fullName))
            query = query.Where(p => p.FullName.Contains(fullName));

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(p => p.FirstName.Contains(firstName));

        if (!string.IsNullOrWhiteSpace(middleName))
            query = query.Where(p => p.MiddleName.Contains(middleName));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(p => p.LastName.Contains(lastName));

        if (!string.IsNullOrWhiteSpace(departmentCodeCode))
            query = query.Where(p => p.Code.Code1 == departmentCodeCode && p.Code.Type == "Department" && !p.Code.Delete);

        if (!string.IsNullOrWhiteSpace(pin))
            query = query.Where(p => p.Account.Contains(pin));

        if (personEntityIds != null)
            query = query.Where(p => personEntityIds.Contains(p.Id));

        var temp = await query
            .Include(p => p.Addresses.Where(a => new[] { "B", "S" }.Contains(a.Code.Code1) && !a.Delete))   // Billing, Service
            .ThenInclude(a => a.Code)
            .Include(p => p.Code)
            .Include(p => p.Emails)
            .Include(p => p.Phones)
            .AsSplitQuery()
            .ToListAsync();

        // addresses
        List<PersonEntity> value = new();
        var bypassBilling = string.IsNullOrWhiteSpace(billingAddress);
        var bypassService = string.IsNullOrWhiteSpace(serviceAddress);
        foreach (var pe in temp)
        {
            if (
                (bypassBilling || FilterAddress(billingAddress, GetAddressesByCode("B", pe.Addresses))) &&
                (bypassService || FilterAddress(serviceAddress, GetAddressesByCode("S", pe.Addresses)))
            )
            {
                value.Add(pe);
            }
        }
        return value;
    }

    private static Address[] GetAddressesByCode(string code, IEnumerable<Address> addresses)
    {
        return addresses == null
            ? Array.Empty<Address>()
            : addresses.Where(a => a.Code.Code1 == code).ToArray();
    }

    private static bool FilterAddress(string search, IEnumerable<Address> addresses)
    {
        if (string.IsNullOrWhiteSpace(search) || !addresses.Any())
            return false;

        var parts = SplitAddress(search);
        foreach(var a in addresses)
        {
            var allPartsMatch = true;

            foreach(var part in parts)
            {
                if (!(
                    MatchesPart(a.StreetName, part) ||
                    MatchesPart(a.Direction, part) ||
                    MatchesPart(a.Suffix, part) ||
                    MatchesPart(a.Apt, part) ||
                    MatchesPart(a.Number?.ToString(), part)
                    ))
                {
                    allPartsMatch = false;
                    break;
                }
            }

            if (allPartsMatch)
                return true;
        }

        return false;
    }

    private static bool MatchesPart(string addressComponent, string searchPart)
    {
        return !string.IsNullOrWhiteSpace(addressComponent)
            && addressComponent.Contains(searchPart);
    }

    private static string[] SplitAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Array.Empty<string>();

        var chars = address.ToCharArray();
        for(int i = 0; i < chars.Length; i++)
        {
            if (char.IsLetterOrDigit(chars[i]))
                continue;

            chars[i] = ' ';
        }
        var str = new string(chars);
        return str
            .Split()
            .Where(s => s.Length > 0)
            .ToArray();
    }

    #endregion

}
