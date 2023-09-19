using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BillBlobs.Services;

public static class SwDbContextExtensions
{
    #region Transactions

    public static Task<DateTime> Get_Last_Bill_Tran_DateTime_For_Prev_Billing(this SwDbContext db, DateTime process_date)
    {
        Transaction transactions = db.Transactions
            .Where(t => t.TransactionCodeId == t.TransactionCode.TransactionCodeId && (t.TransactionCode.Code == "MB" || t.TransactionCode.Code == "MBR" || t.TransactionCode.Code == "FB") && t.AddDateTime < process_date)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .First();

        if (transactions == null)
        {
            throw new InvalidOperationException("No previous Monthly Billing Records Found!");
        }
        else
        {
            return Task.FromResult(transactions.AddDateTime);
        }
    }

    public static Task<DateTime> Get_Last_Bill_Tran_DateTime_For_Curr_Billing(this SwDbContext db, DateTime process_date)
    {
        Transaction transactions = db.Transactions
            .Where(t => t.TransactionCodeId == t.TransactionCode.TransactionCodeId && (t.TransactionCode.Code == "MB" || t.TransactionCode.Code == "MBR" || t.TransactionCode.Code == "FB"))
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .First();

        if (transactions == null)
        {
            throw new InvalidOperationException("No previous Monthly Billing Records Found!");
        }
        else
        {
            return Task.FromResult(transactions.AddDateTime);
        }
    }

    #endregion

    #region Bill Master

    public static Task<List<BillMaster>> GetBillMasterRecordsByDateRange(this SwDbContext db, DateTime beg_datetime, DateTime end_datetime)
    {
        return db.BillMasters
            .Where(t => t.AddDateTime >= beg_datetime && t.AddDateTime <= end_datetime && !t.DeleteFlag)
            .OrderBy(t => t.AddDateTime)
            .ToListAsync();
    }

    #endregion
}
