using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;
using System.ComponentModel.DataAnnotations;

namespace SW.BLL.Services;

public class ContainerCodeService : IContainerCodeService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ContainerCodeService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task Add(ContainerCode containerCode)
    {
        _ = containerCode ?? throw new ArgumentNullException(nameof(containerCode));
        Validator.ValidateObject(containerCode, new ValidationContext(containerCode));

        containerCode.Type = containerCode.Type.ToUpper().Trim();
        containerCode.Description = containerCode.Description.ToUpper().Trim();
        containerCode.BillingFrequency = "M";
        containerCode.AddToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();

        var anyDuplicates = await db.ContainerCodes.AnyAsync(cc => cc.Type == containerCode.Type && !cc.DeleteFlag);
        if (anyDuplicates)
        {
            throw new InvalidOperationException("Container Code already exists.");
        }

        db.ContainerCodes.Add(containerCode);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<ContainerCode>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerCodes
            .Where(c => !c.DeleteFlag)
            .OrderBy(c => c.Type)
            .ToListAsync();
    }

    public async Task<ContainerCode> GetById(int containerCodeId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ContainerCodes
            .Where(e => e.ContainerCodeId == containerCodeId)
            .SingleOrDefaultAsync();
    }

    public async Task Update(ContainerCode containerCode)
    {
        _ = containerCode ?? throw new ArgumentNullException(nameof(containerCode));
        Validator.ValidateObject(containerCode, new ValidationContext(containerCode));

        containerCode.Type = containerCode.Type.ToUpper().Trim();
        containerCode.Description = containerCode.Description.ToUpper().Trim();
        containerCode.BillingFrequency = "M";
        containerCode.ChgDateTime ??= DateTime.Now;
        containerCode.ChgToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        var anyDup = await db.ContainerCodes.AnyAsync(cc => cc.Type == containerCode.Type && !cc.DeleteFlag && cc.ContainerCodeId != containerCode.ContainerCodeId);
        if (anyDup)
            throw new InvalidOperationException("Duplicate container code type");

        db.ContainerCodes.Update(containerCode);
        await db.SaveChangesAsync();
    }

    public async Task Delete(ContainerCode containerCode)
    {
        containerCode.DelDateTime ??= DateTime.Now;
        containerCode.DeleteFlag = true;
        containerCode.DelToi ??= System.Security.Claims.ClaimsPrincipal.Current?.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.ContainerCodes.Update(containerCode);
        await db.SaveChangesAsync();
    }
}
