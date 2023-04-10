using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class CustomerService : ICustomerService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public CustomerService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<Customer>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Customers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Customer> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.GetCustomerById(id);
    }

    public async Task<Customer> GetByPE(int peId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Customers
            .Where(e => e.Pe == peId)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    internal string TryValidateCustomer(Customer customer)
    {
        if (customer.CancelDate.HasValue && customer.CancelDate.Value < customer.EffectiveDate)
            return "Customer cancel date before effective date";

        return null;
    }

    internal void ValidateCustomer(Customer customer)
    {
        var temp = TryValidateCustomer(customer);
        if (!string.IsNullOrWhiteSpace(temp))
            throw new ArgumentException(temp, nameof(customer));
    }

    public async Task Add(Customer customer, string addToi)
    {
        ValidateCustomer(customer);
        customer.AddDateTime = DateTime.Now;
        customer.AddToi = addToi;

        using var db = dbFactory.CreateDbContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
    }

    public async Task Add(Customer customer)
    {
        ValidateCustomer(customer);
        customer.AddDateTime = DateTime.Now;

        using var db = dbFactory.CreateDbContext();
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
    }

    public async Task Update(Customer customer)
    {
        ValidateCustomer(customer);
        customer.ChgDateTime = DateTime.Now;

        using var db = dbFactory.CreateDbContext();
        db.Customers.Update(customer);
        await db.SaveChangesAsync();
    }

    public async Task<int> GetNextCustomerNumber(string customerType)
    {
        using var db = dbFactory.CreateDbContext();
        int? maxCustomerId = await db.Customers
            .Where(e => e.CustomerType == customerType)
            .OrderByDescending(id => id)
            .Select(e => e.CustomerId)
            .FirstOrDefaultAsync();

        if (maxCustomerId is null)
        {
            switch (customerType)
            {
                case "C":
                    return 100000;
                case "H":
                    return 300000;
                case "R":
                    return 1000000;
                default:
                    throw new ArgumentException("No Customer # Ranges found for Customer Type " + customerType + ".", nameof(customerType));
            }
        }

        if (customerType == "C" && maxCustomerId + 1 > 299999)
        {
            throw new ArgumentException("Maximum Range Limit of 299999 Exceeded for Commercial Customer Type.", nameof(customerType));
        }

        if (customerType == "H" && maxCustomerId + 1 > 399999)
        {
            throw new ArgumentException("Maximum Range Limit of 399999 Exceeded for Home Owner Assoc. Customer Type.", nameof(customerType));
        }

        return maxCustomerId.Value + 1;
    }

    public async Task CancelRelatedEntities(Customer c, string chgToi)
    {
        using var db = dbFactory.CreateDbContext();

        var serviceAddresses = await db.ServiceAddresses
            .Where(s => s.CustomerId == c.CustomerId)
            .Include(s => s.Containers)
            .ToListAsync();

        foreach (ServiceAddress sa in serviceAddresses)
        {
            foreach (Container co in sa.Containers)
            {
                var coUpdate = false;
                if (co.CancelDate == null || co.CancelDate > c.CancelDate)
                {
                    co.CancelDate = c.CancelDate;
                    coUpdate = true;
                }
                if (co.EffectiveDate > c.CancelDate)
                {
                    co.EffectiveDate = c.CancelDate.Value;
                    coUpdate = true;
                }
                if (coUpdate)
                {
                    co.ChgToi = chgToi;
                    co.ChgDateTime = DateTime.Now;
                }
            }

            var saUpdate = false;
            if (sa.CancelDate == null || sa.CancelDate > c.CancelDate)
            {
                sa.CancelDate = c.CancelDate;
                saUpdate = true;
            }
            if (sa.EffectiveDate > c.CancelDate)
            {
                sa.EffectiveDate = c.CancelDate.Value;
                saUpdate = true;
            }
            if (saUpdate)
            {
                sa.ChgToi = chgToi;
                sa.ChgDateTime = DateTime.Now;
            }
        }

        await db.SaveChangesAsync();
    }
}
