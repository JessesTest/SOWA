using Common.Extensions;
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

    public async Task Add(PaymentPlan pp)
    {
        _ = pp ?? throw new ArgumentNullException(nameof(pp));
        pp.AddToi ??= System.Security.Claims.ClaimsPrincipal.Current.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.PaymentPlans.Add(pp);
        await db.SaveChangesAsync();
    }

    public async Task Update(PaymentPlan pp)
    {
        _ = pp ?? throw new ArgumentNullException(nameof(pp));
        pp.ChgDateTime ??= DateTime.Now;
        pp.ChgToi ??= System.Security.Claims.ClaimsPrincipal.Current.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.PaymentPlans.Update(pp);
        await db.SaveChangesAsync();
    }

    public async Task<PaymentPlan> GetById(int paymentPlanId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.PaymentPlans
            .Where(p => p.Id == paymentPlanId)
            .Include(p => p.Details)
            .SingleOrDefaultAsync();
    }
}
