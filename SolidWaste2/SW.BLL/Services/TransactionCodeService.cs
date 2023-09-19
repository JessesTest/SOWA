using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionCodeService : ITransactionCodeService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public TransactionCodeService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<TransactionCode>> CollectionPaymentCodes()
    {
        var codes = new[] { "PV", "PCC", "PU", "V2U", "C2U" };
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => codes.Contains(c.Code))
            .OrderBy(c => c.Description)
            .ToListAsync();
    }

    public async Task<ICollection<TransactionCode>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => !c.DeleteFlag)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<ICollection<TransactionCode>> GetAllByGroup(string group)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => !c.DeleteFlag && c.Group == group)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<TransactionCode> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => c.TransactionCodeId == id)
            .SingleOrDefaultAsync();
    }

    public async Task<TransactionCode> GetByCode(string code)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionCodes
            .Where(c => c.Code == code && !c.DeleteFlag)
            .SingleOrDefaultAsync();
    }

    public async Task Add(TransactionCode transactionCode)
    {
        using var db = dbFactory.CreateDbContext();

        ValidateTransactionCode(transactionCode);
        FormatTransactionCode(transactionCode);

        transactionCode.AddDateTime = DateTime.Now;

        var anyDups = await db.TransactionCodes
            .Where(c => c.Code == transactionCode.Code && !c.DeleteFlag)
            .AnyAsync();
        if (anyDups)
            throw new ArgumentException("Transaction Code already exists.");

        db.TransactionCodes.Add(transactionCode);
        await db.SaveChangesAsync();
    }

    public async Task Update(TransactionCode transactionCode)
    {
        using var db = dbFactory.CreateDbContext();

        ValidateTransactionCode(transactionCode);
        FormatTransactionCode(transactionCode);

        transactionCode.ChgDateTime = DateTime.Now;

        var anyDups = await db.TransactionCodes
            .Where(c => c.Code == transactionCode.Code &&
                c.TransactionCodeId != transactionCode.TransactionCodeId
                && !c.DeleteFlag)
            .AnyAsync();
        if (anyDups)
            throw new ArgumentException("Transaction Code already exists.");

        db.TransactionCodes.Update(transactionCode);
        await db.SaveChangesAsync();
    }

    public async Task Delete(TransactionCode transactionCode)
    {
        using var db = dbFactory.CreateDbContext();

        transactionCode.DeleteFlag = true;
        transactionCode.DelDateTime = DateTime.Now;

        db.TransactionCodes.Update(transactionCode);
        await db.SaveChangesAsync();
    }

    #region Utilities

    internal static void ValidateTransactionCode(TransactionCode transactionCode)
    {
        if (string.IsNullOrWhiteSpace(transactionCode.Code))
            throw new ArgumentException("Transaction Code required.");

        if (string.IsNullOrWhiteSpace(transactionCode.Description))
            throw new ArgumentException("Description required.");
    }

    internal static void FormatTransactionCode(TransactionCode tc)
    {
        if (!string.IsNullOrWhiteSpace(tc.Code))
            tc.Code = tc.Code.ToUpper().Trim();
        if (!string.IsNullOrWhiteSpace(tc.Description))
            tc.Description = tc.Description.ToUpper().Trim();

        if (!string.IsNullOrWhiteSpace(tc.TransactionSign))
            tc.TransactionSign = tc.TransactionSign.ToUpper();
        if (!string.IsNullOrWhiteSpace(tc.CollectionsBalanceSign))
            tc.CollectionsBalanceSign = tc.CollectionsBalanceSign.ToUpper();
        if (!string.IsNullOrWhiteSpace(tc.CounselorsBalanceSign))
            tc.CounselorsBalanceSign = tc.CounselorsBalanceSign.ToUpper();
        if (!string.IsNullOrWhiteSpace(tc.AccountType))
            tc.AccountType = tc.AccountType.ToUpper();
    }

    #endregion
}
