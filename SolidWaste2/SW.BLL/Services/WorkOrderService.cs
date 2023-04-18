using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using PE.BL.Services;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;
using System.Security.Cryptography;

namespace SW.BLL.Services;

public class WorkOrderService :IWorkOrderService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly IAddressService _addressService;

    public WorkOrderService(IDbContextFactory<SwDbContext> dbFactory, IAddressService addressService)
    {
        this.dbFactory = dbFactory;
        _addressService = addressService;
    }

    public async Task<WorkOrder> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.WorkOrders
            .Where(e => e.WorkOrderId == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task Add(WorkOrder workOrder)
    {
        using var db = dbFactory.CreateDbContext();

        if (workOrder.ServiceAddressId.HasValue)
        {
            var serviceAddress = await db.GetServiceAddressById(workOrder.ServiceAddressId.Value);
            var address = await _addressService.GetById(serviceAddress.PeaddressId);
            workOrder.CustomerAddress = address.ToFullString();
        }

        await ValidateWorkOrder(workOrder);

        workOrder.TransDate = DateTime.Now;
        workOrder.AddDateTime = DateTime.Now;
        //workOrder.AddToi = user;

        FormatWorkOrder(workOrder);

        db.WorkOrders.Add(workOrder);
        await db.SaveChangesAsync();
        //_workOrderRepository.Add(workOrder);
    }

    public async Task Update(WorkOrder workOrder)
    {
        using var db = dbFactory.CreateDbContext();

        if (workOrder.ServiceAddressId.HasValue)
        {
            var serviceAddress = await db.GetServiceAddressById(workOrder.ServiceAddressId.Value);
            var address = await _addressService.GetById(serviceAddress.PeaddressId);
            workOrder.CustomerAddress = address.ToFullString();
        }

        await ValidateWorkOrder(workOrder);

        //WorkOrder old = GetWorkOrderById(workOrder.WorkOrderId);

        //workOrder.AddDateTime = old.AddDateTime;
        //workOrder.AddToi = old.AddToi;
        workOrder.ChgDateTime = DateTime.Now;
        //workOrder.ChgToi = user;

        FormatWorkOrder(workOrder);

        db.WorkOrders.Update(workOrder);
        await db.SaveChangesAsync();
        //_workOrderRepository.Update(workOrder);
    }

    public async Task Delete(WorkOrder workOrder)
    {
        using var db = dbFactory.CreateDbContext();

        //if (workOrder.ServiceAddressId.HasValue)
        //{
        //    var serviceAddress = await db.GetServiceAddressById(workOrder.ServiceAddressId.Value);
        //    var address = await _addressService.GetById(serviceAddress.PeaddressId);
        //    workOrder.CustomerAddress = address.ToFullString();
        //}

        //await ValidateWorkOrder(workOrder);

        //WorkOrder old = GetWorkOrderById(workOrder.WorkOrderId);

        //workOrder.AddDateTime = old.AddDateTime;
        //workOrder.AddToi = old.AddToi;
        //workOrder.ChgDateTime = old.ChgDateTime;
        //workOrder.ChgToi = old.ChgToi;
        workOrder.DelFlag = true;
        workOrder.DelDateTime = DateTime.Now;
        //workOrder.DelToi = user;

        FormatWorkOrder(workOrder);

        db.WorkOrders.Update(workOrder);
        await db.SaveChangesAsync();
        //_workOrderRepository.Update(workOrder);
    }

    public async Task<ICollection<WorkOrder>> GetInquiryResultList(int? workOrderId, string containerRoute, DateTime? transDate, string driverInitials, string customerName, string customerAddress, bool include)
    {
        using var db = dbFactory.CreateDbContext();

        IQueryable<WorkOrder> query = db.WorkOrders
           .Where(e => !e.DelFlag)
           .AsNoTracking();

        if (workOrderId.HasValue)
            query = query.Where(e => e.WorkOrderId.ToString().Contains(workOrderId.ToString()));

        if (!string.IsNullOrWhiteSpace(containerRoute))
            query = query.Where(e => e.ContainerRoute == containerRoute);

        if (transDate.HasValue)
            query = query.Where(e => e.TransDate == transDate);

        if (!string.IsNullOrWhiteSpace(driverInitials))
            query = query.Where(e => e.DriverInitials == driverInitials);

        if (!string.IsNullOrWhiteSpace(customerName))
            query = query.Where(e => e.CustomerName.Contains(customerName));

        if (!string.IsNullOrWhiteSpace(customerAddress))
            query = query.Where(e => e.CustomerAddress.Contains(customerAddress));

        if (include)        //this is checked so show closed work orders   (yes   resolved date)
            query = query.Where(e => e.ResolveDate.HasValue);

        if (!include)       //this is unchecked so show open work orders   (no   resolved date)
            query = query.Where(e => !e.ResolveDate.HasValue);

        return await query
            .Take(500)
            .ToListAsync();
    }

    #region Utility

    internal async Task ValidateWorkOrder(WorkOrder workOrder)
    {
        using var db = dbFactory.CreateDbContext();

        // Make sure CustomerType is provided
        if (string.IsNullOrWhiteSpace(workOrder.CustomerType))
            throw new ArgumentException("[CustomerType] required");

        // Make sure CustomerType is valid
        if (!"$C$ $H$ $R$".Contains(string.Format("${0}$", workOrder.CustomerType)))
            throw new ArgumentException("[CustomerType] invalid");

        // Extra validation if CustomerId provided
        if (workOrder.CustomerId.HasValue)
        {
            // Make sure CustomerId is valid
            //Customer customer = _customerRepository.GetSingle(m => m.CustomerID == workOrder.CustomerId.Value || m.LegacyCustomerID == workOrder.CustomerId);
            var customer = await db.GetCustomerById(workOrder.CustomerId.Value);
            if (customer == null)
                throw new ArgumentException("[CustomerId] invalid");

            // Extra validation if ServiceAddressId provided
            if (workOrder.ServiceAddressId.HasValue)
            {
                // Make sure ServiceAddressId is valid
                //ServiceAddress serviceAddress = _serviceAddressRepository.GetSingle(m => m.Id == workOrder.ServiceAddressId && !m.DeleteFlag);
                var serviceAddress = await db.GetServiceAddressById(workOrder.ServiceAddressId.Value);
                if (serviceAddress == null || serviceAddress.DeleteFlag)
                    throw new ArgumentException("[ServiceAddressId] invalid");

                // Make sure ServiceAddress belongs to Customer
                if (serviceAddress.CustomerId != customer.CustomerId)
                    throw new ArgumentException("[ServiceAddressId] does not belong to provided Customer");

                // Extra validation if ContainerId provided
                if (workOrder.ContainerId.HasValue)
                {
                    // Make sure ContainerId is valid
                    //Container container = _containerRepository.GetSingle(m => m.Id == workOrder.ContainerId && !m.DeleteFlag);
                    var container = await db.GetContainerById(workOrder.ContainerId.Value);
                    if (container == null || container.DeleteFlag)
                        throw new ArgumentException("[ContainerId] invalid");

                    // Make sure Container belongs to ServiceAddress
                    if (container.ServiceAddressId != serviceAddress.Id)
                        throw new ArgumentException("[ContainerId] does not belong to provided ServiceAddress");
                }
            }
        }

        // Extra validation if CustomerType is commercial
        // Make sure CustomerName is provided if CustomerType is commercial
        if (workOrder.CustomerType == "C" && string.IsNullOrWhiteSpace(workOrder.CustomerName))
            throw new ArgumentException("[CustomerName] required for commercial customers");

        // Make sure CustomerAddress is provided
        if (string.IsNullOrWhiteSpace(workOrder.CustomerAddress))
            throw new ArgumentException("[CustomerAddress] required");

        // Make sure DriverInitials is provided
        if (string.IsNullOrWhiteSpace(workOrder.DriverInitials))
            throw new ArgumentException("[DriverInitials] required");

        // Make sure ContainerRoute is provided
        if (string.IsNullOrWhiteSpace(workOrder.ContainerRoute))
            throw new ArgumentException("[ContainerRoute] required");

        // Make sure ContainerCode is provided
        if (string.IsNullOrWhiteSpace(workOrder.ContainerCode))
            throw new ArgumentException("[ContainerCode] required");

        // Make sure ContainerSize is provided
        if (!workOrder.ContainerSize.HasValue)
            throw new ArgumentException("[ContainerSize] required");

        // Make sure RepairsNeeded is provided
        if (string.IsNullOrWhiteSpace(workOrder.RepairsNeeded))
            throw new ArgumentException("[RepairsNeeded] required");
    }

    internal static void FormatWorkOrder(WorkOrder workOrder)
    {
        if (!string.IsNullOrWhiteSpace(workOrder.AddToi))
            workOrder.AddToi = workOrder.AddToi.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.ChgToi))
            workOrder.ChgToi = workOrder.ChgToi.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.ContainerCode))
            workOrder.ContainerCode = workOrder.ContainerCode.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.ContainerRoute))
            workOrder.ContainerRoute = workOrder.ContainerRoute.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.CustomerAddress))
            workOrder.CustomerAddress = workOrder.CustomerAddress.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.CustomerName))
            workOrder.CustomerName = workOrder.CustomerName.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.CustomerType))
            workOrder.CustomerType = workOrder.CustomerType.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.DelToi))
            workOrder.DelToi = workOrder.DelToi.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.DriverInitials))
            workOrder.DriverInitials = workOrder.DriverInitials.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.RepairsNeeded))
            workOrder.RepairsNeeded = workOrder.RepairsNeeded.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(workOrder.ResolutionNotes))
            workOrder.ResolutionNotes = workOrder.ResolutionNotes.ToUpper().Trim();
    }

    #endregion
}
