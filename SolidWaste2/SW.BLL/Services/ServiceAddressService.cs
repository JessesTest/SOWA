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
    private readonly IContainerService containerService;

    public ServiceAddressService(
        IDbContextFactory<SwDbContext> dbFactory,
        IAddressService addressService,
        IContainerService containerService)
    {
        this.dbFactory = dbFactory;
        this.addressService = addressService;
        this.containerService = containerService;
    }

    public async Task<ServiceAddress> GetById(int serviceAddressId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ServiceAddresses
            .Where(e => e.Id == serviceAddressId)
            .Include(e => e.Customer)
            .Include(e => e.Containers)
            .Include(e => e.ServiceAddressNotes)
            .Include(e => e.BillServiceAddresses)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task CustomerCancelServiceAddress(DateTime cancelDate, PersonEntity person, int serviceAddressId, string userName)
    {
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
        serviceAddress.ChgDateTime = DateTime.Now;
        serviceAddress.ChgToi = userName;

        var containers = await db.Containers
            .Where(c => !c.DeleteFlag && c.ServiceAddressId == serviceAddress.Id)
            .Include(c => c.ContainerCode)
            .ToListAsync();

        foreach (var container in containers)
        {
            await CustomerCancelPart(db, container, serviceAddress, cancelDate, person, userName);
        }

        await db.SaveChangesAsync();
    }

    private async Task CustomerCancelPart(
            SwDbContext db,
            Container container,
            ServiceAddress serviceAddress,
            DateTime cancelDate,
            PersonEntity person,
            string userName)
    {
        if (container.CancelDate.HasValue && container.CancelDate <= cancelDate)
        {
            return;
        }

        // check for work orders
        var workOrdersToCancel = await db.WorkOrders
            .Where(w => !w.DelFlag && w.ContainerId == container.Id && w.TransDate > cancelDate)
            .ToListAsync();
        foreach (var wo in workOrdersToCancel)
        {
            wo.DelFlag = true;
            wo.DelDateTime = DateTime.Now;
            wo.DelToi = userName;
        }

        var peaddress = await addressService.GetById(serviceAddress.PeaddressId);

        // new work order
        var o = new WorkOrder
        {
            AddDateTime = DateTime.Now,
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
        container.ChgDateTime = DateTime.Now;
        container.ChgToi = userName;
        container.Delivered = "Scheduled for Pick Up";
    }

    public async Task<ICollection<ServiceAddress>> GetByCustomer(int customerId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ServiceAddresses
            .Where(e => e.CustomerId == customerId && !e.DeleteFlag)
            .Include(e => e.Customer)
            .Include(e => e.Containers)
            .Include(e => e.ServiceAddressNotes)
            .Include(e => e.BillServiceAddresses)
            .AsNoTracking()
            .ToListAsync();
    }
}
