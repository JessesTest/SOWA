using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using PE.DM;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class CustomerInquiryService  : ICustomerInquiryService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly IPersonEntityService personEntityService;

    public CustomerInquiryService(
        IDbContextFactory<SwDbContext> dbFactory,
        IPersonEntityService personEntityService)
    {
        this.dbFactory = dbFactory;
        this.personEntityService = personEntityService;
    }

    public async Task<ICollection<CustomerInquiryResult>> Search(
        int? customerNumber,
        string customerName,
        string customerAddress,
        string locationName,
        string PIN,
        string locationAddress,
        bool include)
    {
        ICollection<int> personEntityIds = null;

        if (customerNumber != null)
        {
            personEntityIds = await GetPersonEntityIdsByCustomerId(customerNumber, include);
            if (!personEntityIds.Any())
                return Array.Empty<CustomerInquiryResult>();
        }

        if (!string.IsNullOrWhiteSpace(locationName))
        {
            var temp = await GetPersonEntityIdsByLocation(locationName, include);
            if (!temp.Any())
                return Array.Empty<CustomerInquiryResult>();

            personEntityIds = personEntityIds == null ? temp : personEntityIds.Intersect(temp).ToList();
        }

        var personEntities = await CustomerInquiry_PE(customerName, customerAddress, locationAddress, PIN, personEntityIds);
        if (!personEntities.Any())
            return Array.Empty<CustomerInquiryResult>();

        var peIds = personEntities.Select(p => p.Id).ToList();
        var customers = await GetCustomersByPe(peIds, include);
        if (!customers.Any())
            return Array.Empty<CustomerInquiryResult>();

        List<CustomerInquiryResult> value = new();
        foreach(var customer in customers)
        {
            var personEntity = personEntities.Single(p => p.Id == customer.Pe);
            var billingAddress = personEntity.Addresses.FirstOrDefault(a => a.Code.Code1 == "B" && !a.Delete);

            foreach (var sa in customer.ServiceAddresses)
            {
                sa.PEAddress = personEntity.Addresses.FirstOrDefault(pea => pea.Id == sa.PeaddressId);
            }

            CustomerInquiryResult item = new()
            {
                BillingAddress = billingAddress,
                Customer = customer,
                PersonEntity = personEntity,
                ServiceAddresses = customer.ServiceAddresses
            };
            value.Add(item);
        }
        return value.OrderBy(c=> c.Customer.EffectiveDate).Take(500).ToList();
    }

    private async Task<ICollection<PersonEntity>> CustomerInquiry_PE(
        string customerName,
        string customerAddress,
        string locationAddress,
        string pin,
        IEnumerable<int> personEntityIds
        )
    {
        if (pin == null)
        {
            pin = "";
        }
        else if (pin.StartsWith("SW"))
        {
            pin = pin.Substring(2);
        }

        return await personEntityService.Search(
            customerName,
            "",
            "",
            "",
            customerAddress,
            locationAddress,
            "SW",
            pin,
            personEntityIds);
    }

    private async Task<ICollection<int>> GetPersonEntityIdsByCustomerId(int? customerNumber, bool includeInactive)
    {
        if (customerNumber == null || customerNumber <= 0)
            return null;

        using var db = dbFactory.CreateDbContext();

        var query = db.Customers
            .Where(c => c.CustomerId == customerNumber || c.LegacyCustomerId == customerNumber);

        if (!includeInactive)
            query = query.Where(c => c.CancelDate == null);

        return await query.Select(c => c.Pe).ToListAsync();

    }

    private async Task<ICollection<int>> GetPersonEntityIdsByLocation(string locationName, bool includeInactive)
    {
        using var db = dbFactory.CreateDbContext();

        var query = db.ServiceAddresses
            .Where(a => a.LocationName.Contains(locationName))
            .Select(a => a.Customer);

        if (!includeInactive)
            query = query.Where(c => c.CancelDate == null);

        return await query.Select(c => c.Pe).ToListAsync();
    }

    public async Task<ICollection<Customer>> GetCustomersByPe(ICollection<int> peIds, bool includeInactive)
    {
        using var db = dbFactory.CreateDbContext();
        IQueryable<Customer> query = db.Customers
            .Where(c => peIds.Contains(c.Pe));

        if (includeInactive)
            query = query.Where(c => c.CancelDate == null);

        return await query
            .OrderByDescending(c => c.EffectiveDate)
            .Include(c => c.ServiceAddresses)
            .Take(500)
            .ToListAsync();
    }
}
