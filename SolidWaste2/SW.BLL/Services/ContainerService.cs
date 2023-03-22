using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using PE.DM;
using SW.DAL.Contexts;
using SW.DM;
using System;

namespace SW.BLL.Services;

public class ContainerService : IContainerService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly IAddressService addressService;

    public ContainerService(IDbContextFactory<SwDbContext> dbFactroy, IAddressService addressService)
    {
        this.dbFactory = dbFactroy;
        this.addressService = addressService;
    }

    public async Task<Container> GetById(int containerId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Containers
            .Where(e => e.Id == containerId)
            .Include(e => e.ServiceAddress)
            .Include(e => e.ContainerCode)
            .Include(e => e.ContainerSubtype)
            .Include(e => e.BillContainerDetails)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Container> GetById(int containerId, int customerId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Containers
            .Where(e => e.Id == containerId)
            .Where(e => e.ServiceAddress.CustomerId == customerId)
            .Include(e => e.ServiceAddress)
            .Include(e => e.ContainerCode)
            .Include(e => e.ContainerSubtype)
            .Include(e => e.BillContainerDetails)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task CustomerCancelContainer(DateTime cancelDate, PersonEntity person, int containerId, string userName)
    {
        using var db = dbFactory.CreateDbContext();

        var customer = await db.Customers
            .Where(c => !c.DelDateTime.HasValue && c.Pe == person.Id)
            .FirstOrDefaultAsync()
            ?? throw new ArgumentException("Customer record not found", nameof(person));

        var container = await db.Containers
            .Where(c => !c.DelDateTime.HasValue && c.Id == containerId)
            .Include(c => c.ContainerCode)
            .FirstOrDefaultAsync()
            ?? throw new ArgumentException("Container not found", nameof(containerId));

        var serviceAddress = await db.ServiceAddresses
            .Where(a => !a.DeleteFlag && a.Id == container.ServiceAddressId)
            .FirstOrDefaultAsync()
            ?? throw new ArgumentException("Service address not found", nameof(containerId));

        if (serviceAddress.CustomerId != customer.CustomerId)
            throw new InvalidOperationException("Service address not found");

        await CustomerCancelPart(db, container, serviceAddress, cancelDate, person, userName);

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

    public async Task<ICollection<Container>> GetByCustomer(int customerId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Containers
            .Where(c => !c.DeleteFlag && c.ServiceAddress.CustomerId == customerId)
            .Include(c => c.ServiceAddress)
            .Include(c => c.ContainerCode)
            .Include(c => c.ContainerSubtype)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<Container>> GetByServiceAddress(int serviceAddressId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Containers
            .Where(c => !c.DeleteFlag && c.ServiceAddressId == serviceAddressId)
            .Include(c => c.ServiceAddress)
            .Include(c => c.ContainerCode)
            .Include(c => c.ContainerSubtype)
            .AsNoTracking()
            .ToListAsync();
    }
}
