using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;
using System.ComponentModel.DataAnnotations;

namespace SW.BLL.Services;

public class ContainerRateService : IContainerRateService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ContainerRateService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task Add(ContainerRate containerRate)
    {
        _ = containerRate ?? throw new ArgumentNullException(nameof(containerRate));
        Validator.ValidateObject(containerRate, new ValidationContext(containerRate));

        containerRate.AddToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();
        containerRate.RateDescription = containerRate.RateDescription.ToUpper();

        using var db = dbFactory.CreateDbContext();

        var anyDups = await db.ContainerRates
            .Where(c => c.ContainerType == containerRate.ContainerType)
            .Where(c => c.ContainerSubtypeId == containerRate.ContainerSubtypeId)
            .Where(c => c.BillingSize == containerRate.BillingSize)
            .Where(c => c.NumDaysService == containerRate.NumDaysService)
            .Where(c => c.EffectiveDate == containerRate.EffectiveDate)
            //.Where(c => c.ContainerRateId != containerRate.ContainerRateId)
            .Where(c => !c.DeleteFlag)
            .AnyAsync();
        if (anyDups)
            throw new InvalidOperationException("Duplicate container rate");

        db.ContainerRates.Add(containerRate);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<ContainerRate>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(e => !e.DeleteFlag)
            .ToListAsync();
    }

    public async Task<ContainerRate> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(e => e.ContainerRateId == id)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<ContainerRate>> GetContainerRateByCodeDaysSize(
        int containerSubtypeId,
        int dayCount,
        decimal billingSize,
        DateTime effective_date)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(c => c.ContainerSubtypeId == containerSubtypeId
            && c.NumDaysService == dayCount
            && c.BillingSize == billingSize
            && !c.DeleteFlag
            && c.EffectiveDate <= effective_date)
            .ToListAsync();
    }

    public async Task<ICollection<ContainerRate>> GetByCodeDaysSizeEffDate(int containerCodeId, int containerSubtypeId, int dayCount, decimal billingSize, DateTime effectiveDate)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(c => c.ContainerType == containerCodeId
            && c.ContainerSubtypeId == containerSubtypeId
            && c.NumDaysService == dayCount
            && c.BillingSize == billingSize
            && c.EffectiveDate <= effectiveDate
            && !c.DeleteFlag)
            .OrderByDescending(c => c.EffectiveDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<ContainerRate>> GetContainerRateByCodeDays(int containerCodeId, int dayCount)
    {
        using var db = dbFactory.CreateDbContext();

        return await db.ContainerRates
            .Where(e => e.ContainerType == containerCodeId && e.NumDaysService == dayCount && !e.DeleteFlag)
            .OrderBy(e => e.EffectiveDate)
            .ThenBy(e => e.ContainerSubtypeId)
            .ThenBy(e => e.BillingSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<int>> GetDaysOfServiceByContainerSubtype(int containerSubtypeId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(e => !e.DeleteFlag && e.ContainerSubtypeId == containerSubtypeId && e.NumDaysService != null)
            .Select(e => e.NumDaysService.Value)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();
    }

    public async Task<ICollection<ContainerRateListing>> GetListing()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerRates
            .Where(r => !r.DeleteFlag)
            .Select(r => new ContainerRateListing
            {
                Amount = r.RateAmount,
                BillingSize = r.BillingSize,
                ContainerRateId = r.ContainerRateId,
                Description = r.RateDescription,
                EffectiveDate = r.EffectiveDate,
                NumDaysService = r.NumDaysService,
                PullCharge = r.PullCharge,
                SubtypeDescription = r.ContainerSubtype.Description,
                SubtypeFrequency = r.ContainerSubtype.BillingFrequency,
                TypeCode = r.ContainerTypeNavigation.Type,
                TypeDescription = r.ContainerTypeNavigation.Description
            })
            .ToListAsync();
    }

    public async Task Update(ContainerRate containerRate)
    {
        _ = containerRate ?? throw new ArgumentNullException(nameof(containerRate));
        Validator.ValidateObject(containerRate, new ValidationContext(containerRate));

        containerRate.ChgDateTime ??= DateTime.Now;
        containerRate.ChgToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();
        containerRate.RateDescription = containerRate.RateDescription.ToUpper();

        using var db = dbFactory.CreateDbContext();

        var anyDup = await db.ContainerRates
            .Where(c => c.ContainerType == containerRate.ContainerType)
            .Where(c => c.ContainerSubtypeId == containerRate.ContainerSubtypeId)
            .Where(c => c.BillingSize == containerRate.BillingSize)
            .Where(c => c.NumDaysService == containerRate.NumDaysService)
            .Where(c => c.EffectiveDate == containerRate.EffectiveDate)
            .Where(c => c.ContainerRateId != containerRate.ContainerRateId)
            .Where(c => !c.DeleteFlag)
            .AnyAsync();
        if (anyDup)
        {
            throw new InvalidOperationException("Container Rate already exists.");
        }

        db.ContainerRates.Update(containerRate);
        await db.SaveChangesAsync();
    }

    public async Task Delete(ContainerRate containerRate)
    {
        _ = containerRate ?? throw new ArgumentNullException(nameof(containerRate));
        Validator.ValidateObject(containerRate, new ValidationContext(containerRate));

        containerRate.DelDateTime ??= DateTime.Now;
        containerRate.DeleteFlag = true;
        containerRate.DelToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.ContainerRates.Update(containerRate);
        await db.SaveChangesAsync();
    }
}
