using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using SW.DAL.Contexts;
using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BLL.Services;

public class WorkOrderService :IWorkOrderService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public WorkOrderService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
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
}
