using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.IfasCashReceipt.Services;

public sealed class BillContainerDetailRepository
{
    private readonly SwDbContext db;

    public BillContainerDetailRepository(IDbContextFactory<SwDbContext> dbFactory)
    {
        db = dbFactory.CreateDbContext();
    }

    public Task SaveChangesAsync()
    {
        return db.SaveChangesAsync();
    }

    public Task<BillContainerDetail> GetFirstUnpaid(int chargeId, int containerId)
    {
        return db.BillContainerDetails
            .Where(t =>
                t.BillServiceAddress.BillMaster.TransactionId == chargeId &&
                t.ContainerId == containerId &&
                t.PaidFull != true)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<BillContainerDetail>> GetByTransaction(int transactionId)
    {
        return await db.BillContainerDetails
            .Where(d => d.BillServiceAddress.BillMaster.TransactionId == transactionId)
            .Where(d => !d.DeleteFlag)
            .OrderBy(d => d.AddDateTime)
            .ThenBy(d => d.BillContainerDetailId)
            .ToListAsync();
    }

    public Task<List<BillContainerDetail>> GetUpdatedContainerDetailByAddDateTimeRangeForCashReceipt(
        DateTime cash_receipt_beg_datetime,
        DateTime cash_receipt_end_datetime)
    {
        return db.BillContainerDetails
            .Where(t =>
                t.ChgDateTime >= cash_receipt_beg_datetime &&
                t.ChgDateTime <= cash_receipt_end_datetime &&
                t.ChgToi == "Cash Receipt" &&
                !t.DeleteFlag)
            .ToListAsync();
    }
}
