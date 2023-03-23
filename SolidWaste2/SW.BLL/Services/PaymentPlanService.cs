using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class PaymentPlanService : IPaymentPlanService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public PaymentPlanService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<PaymentPlan>> GetByCustomer(int customerId, bool includeDetails = true)
    {
        using var db = dbFactory.CreateDbContext();
        IQueryable<PaymentPlan> query = db.PaymentPlans
            .Where(e => e.CustomerId == customerId && !e.DelFlag);

        if (includeDetails)
            query = query.Include(e => e.Details);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<PaymentPlan> GetActiveByCustomer(int customerId, bool includeDetails = true)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.GetActivePaymentPlanByCustomer(customerId, includeDetails);
    }
}
