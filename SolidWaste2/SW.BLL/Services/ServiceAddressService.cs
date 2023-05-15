using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using PE.DM;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class ServiceAddressService : IServiceAddressService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly IAddressService addressService;
    private readonly IPersonEntityService personService;

    public ServiceAddressService(
        IDbContextFactory<SwDbContext> dbFactory,
        IAddressService addressService,
        IPersonEntityService personService)
    {
        this.dbFactory = dbFactory;
        this.addressService = addressService;
        this.personService = personService;
    }

    public async Task<ServiceAddress> GetById(int serviceAddressId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ServiceAddresses
            .Where(e => e.Id == serviceAddressId)
            .Include(e => e.Customer)
            .Include(e => e.Containers.OrderBy(c => c.CancelDate))
            .Include(e => e.ServiceAddressNotes.OrderByDescending(a => a.AddDateTime))
            .Include(e => e.BillServiceAddresses)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task CustomerCancelServiceAddress(DateTime cancelDate, PersonEntity person, int serviceAddressId, string userName)
    {
        var now = DateTime.Now;

        using var db = dbFactory.CreateDbContext();

        var customer = await db.Customers
            .Where(c => !c.DelDateTime.HasValue && c.Pe == person.Id)
            .FirstOrDefaultAsync()
            ?? throw new ArgumentException("Customer record not found", nameof(person));

        var serviceAddress = await db.ServiceAddresses
            .Where(a => !a.DeleteFlag && a.CustomerId == customer.CustomerId && a.Id == serviceAddressId)
            .FirstOrDefaultAsync()
            ?? throw new ArgumentException("Service address not found", nameof(serviceAddressId));

        if (serviceAddress.CancelDate.HasValue && serviceAddress.CancelDate.Value <= DateTime.Now)
            throw new InvalidOperationException("Service address already canceled");

        serviceAddress.CancelDate = cancelDate;
        serviceAddress.ChgDateTime = now;
        serviceAddress.ChgToi = userName;

        var containers = await db.Containers
            .Where(c => !c.DeleteFlag && c.ServiceAddressId == serviceAddress.Id)
            .Include(c => c.ContainerCode)
            .ToListAsync();

        var peaddress = await addressService.GetById(serviceAddress.PeaddressId);

        foreach (var container in containers)
        {
            await CancelContainerAndHandleWorkOrders(db, container, serviceAddress, person, userName, now, peaddress);
        }

        await db.SaveChangesAsync();
    }

    private async Task<bool> CancelContainerAndHandleWorkOrders(
            SwDbContext db,
            Container container,
            ServiceAddress serviceAddress,
            PersonEntity person,    // FullName
            string userName,
            DateTime now,
            Address peaddress)      // address line
    {
        var cancelDate = serviceAddress.CancelDate;

        if (container.CancelDate.HasValue && container.CancelDate <= cancelDate)
        {
            return false;
        }

        // check for work orders
        var workOrdersToCancel = await db.WorkOrders
            .Where(w => !w.DelFlag && w.ContainerId == container.Id && w.TransDate > cancelDate)
            .ToListAsync();
        foreach (var wo in workOrdersToCancel)
        {
            wo.DelFlag = true;
            wo.DelDateTime = now;
            wo.DelToi = userName;
        }

        // new work order
        var o = new WorkOrder
        {
            AddDateTime = now,
            AddToi = userName,
            //o.ChgDateTime
            //o.ChgToi
            //o.Container
            ContainerCode = container.ContainerCode.Type,
            ContainerId = container.Id,
            ContainerPickupFri = container.FriService,
            ContainerPickupMon = container.MonService,
            ContainerPickupSat = container.SatService,
            ContainerPickupThu = container.ThuService,
            ContainerPickupTue = container.TueService,
            ContainerPickupWed = container.WedService,
            //o.ContainerRoute
            ContainerSize = container.BillingSize,
            //o.Customer 
            CustomerAddress = string.Format("{0} {1} {2} {3} {4}", peaddress.Number, peaddress.Direction, peaddress.StreetName, peaddress.Suffix, peaddress.Apt),
            CustomerId = serviceAddress.CustomerId,
            CustomerName = person.FullName,
            CustomerType = serviceAddress.CustomerType,
            //o.DelDateTime 
            //o.DelFlag
            //o.DelToi
            DriverInitials = "",
            RecyclingFlag = (new int[] { 2, 5 }).Contains(container.ContainerCodeId), // what could go wrong?
            RepairsNeeded = string.Format("Customer cancel service {0:d}", cancelDate),
            ResolutionNotes = "",
            ResolveDate = null,
            //o.ServiceAddress
            ServiceAddressId = serviceAddress.Id,
            TransDate = cancelDate
            //o.WorkOrderId
        };
        db.WorkOrders.Add(o);

        container.CancelDate = cancelDate;
        container.ChgDateTime = now;
        container.ChgToi = userName;
        container.Delivered = "Scheduled for Pick Up";

        return true;
    }

    public async Task<ICollection<ServiceAddress>> GetByCustomer(int customerId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ServiceAddresses
            .Where(e => e.CustomerId == customerId && !e.DeleteFlag)
            .Include(e => e.Customer)
            .Include(e => e.Containers.OrderBy(c => c.CancelDate))
            .Include(e => e.ServiceAddressNotes.OrderByDescending(a => a.AddDateTime))
            .Include(e => e.BillServiceAddresses)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Update(ServiceAddress serviceAddress)
    {
        var customer = serviceAddress.Customer;
        var containers = serviceAddress.Containers;
        var notes = serviceAddress.ServiceAddressNotes;
        var bills = serviceAddress.BillServiceAddresses;
        var holdings = serviceAddress.TransactionHoldings;
        var transactions = serviceAddress.Transactions;
        var work = serviceAddress.WorkOrders;
        var now = DateTime.Now;

        serviceAddress.Customer = null;
        serviceAddress.Containers = null;
        serviceAddress.ServiceAddressNotes = null;
        serviceAddress.BillServiceAddresses = null;
        serviceAddress.TransactionHoldings = null;
        serviceAddress.Transactions = null;
        serviceAddress.WorkOrders = null;

        serviceAddress.ChgDateTime ??= now;
        serviceAddress.ChgToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.ServiceAddresses.Update(serviceAddress);

        if(serviceAddress.CancelDate != null && containers != null && containers.Any())
        {
            customer ??= await db.Customers.FirstAsync(c => c.CustomerId == serviceAddress.CustomerId);
            var person = await personService.GetById(customer.Pe);
            var peaddress = await addressService.GetById(serviceAddress.PeaddressId);

            foreach(var container in containers)
            {
                var updatedContainer = await CancelContainerAndHandleWorkOrders(db, container, serviceAddress, person, serviceAddress.ChgToi, now, peaddress);

                if(updatedContainer)
                {
                    db.Containers.Update(container);
                }
            }
        }

        await db.SaveChangesAsync();



        serviceAddress.Customer = customer;
        serviceAddress.Containers = containers;
        serviceAddress.ServiceAddressNotes = notes;
        serviceAddress.BillServiceAddresses = bills;
        serviceAddress.TransactionHoldings = holdings;
        serviceAddress.Transactions = transactions;
        serviceAddress.WorkOrders = work;
    }

    public async Task<string> TryValidateServiceAddress(ServiceAddress sa)
    {
        if (sa.CancelDate.HasValue && DateTime.Today.Date > sa.EffectiveDate && sa.CancelDate < DateTime.Today.Date)
            return ("Service address Cancel Date before " + DateTime.Today.Date.ToShortDateString());


        Customer c = sa.Customer;
        if(c == null)
        {
            using var db = dbFactory.CreateDbContext();
            c = await db.Customers.Where(c => c.CustomerId == sa.CustomerId).FirstOrDefaultAsync();
            if (c == null)
                return "Customer not found";
        }

        if (sa.EffectiveDate < c.EffectiveDate)
            return "Service address effective date before customer effective date";
        if (c.CancelDate.HasValue && !sa.CancelDate.HasValue)
            return "Service address does not have cancel date";
        if (sa.CancelDate.HasValue && c.CancelDate.HasValue && sa.CancelDate.Value > c.CancelDate.Value)
            return "Service address cancel date after customer cancel date";
        if (c.CancelDate.HasValue && sa.EffectiveDate > c.CancelDate.Value)
            return "Service address effective date after customer cancel date";

        return null;
    }

    public async Task Add(ServiceAddress serviceAddress)
    {
        var additionalError = await TryValidateServiceAddress(serviceAddress);
        if (additionalError != null)
            throw new InvalidOperationException(additionalError);

        serviceAddress.LocationNumber = await GenerateLocationNumber(serviceAddress);

        using var db = dbFactory.CreateDbContext();
        db.ServiceAddresses.Add(serviceAddress);
        await db.SaveChangesAsync();
    }
    private async Task<string> GenerateLocationNumber(ServiceAddress s)
    {
        using var db = dbFactory.CreateDbContext();

        var max = await db.ServiceAddresses
            .Where(a => a.CustomerId == s.CustomerId)
            .MaxAsync(a => a.LocationNumber);

        if (max == null)
            return "01";

        var number = int.Parse(max) + 1;

        return $"{number:00}";
    }
}
