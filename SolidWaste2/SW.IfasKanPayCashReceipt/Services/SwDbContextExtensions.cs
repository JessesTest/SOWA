using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.IfasKanPayCashReceipt.Services;

public static class SwDbContextExtensions
{
    #region BillContainerDetail

    public static Task<List<BillContainerDetail>> Get_Bill_Container_Detail_For_Cash_Receipt(
        this SwDbContext db,
        int chargeId)
    {
        return db.BillContainerDetails
            .Where(d => d.BillServiceAddress.BillMaster.TransactionId == chargeId)
            .OrderBy(d => d.AddDateTime)
            .ThenBy(d => d.BillContainerDetailId)
            .ToListAsync();
    }

    public static Task<BillContainerDetail> GetBillContainerDetails(
        this SwDbContext db,
        int chargeId,
        int containerId)
    {
        return db.BillContainerDetails
            .Where(t => t.BillServiceAddress.BillMaster.TransactionId == chargeId)
            .Where(t => t.ContainerId == containerId)
            .Where(t => t.PaidFull != true)
            .FirstOrDefaultAsync();
    }

    public static Task<List<BillContainerDetail>> GetUpdatedContainerDetailByAddDateTimeRangeForCashReceipt(
        this SwDbContext db,
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

    #endregion

    #region Container

    public static Task<Container> GetContainer(this SwDbContext db, int containerId)
    {
        return db.Containers
            .Where(c => c.Id == containerId)
            .Include(c => c.ContainerCode)
            .Include(c => c.ContainerSubtype)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region ContainerRate

    public static Task<ContainerRate> GetContainerRate(
        this SwDbContext db,
        int containerCodeId,
        int containerSubtypeID,
        int dayCount,
        decimal billingSize,
        DateTime effectiveDate)
    {
        return db.ContainerRates
            .Where(c =>
                c.ContainerType == containerCodeId &&
                c.ContainerSubtypeId == containerSubtypeID &&
                c.NumDaysService == dayCount &&
                c.BillingSize == billingSize &&
                c.EffectiveDate <= effectiveDate &&
                !c.DeleteFlag)
            .OrderByDescending(t => t.EffectiveDate)
            .SingleOrDefaultAsync();
    }

    #endregion

    #region Customer

    public static Task<List<Customer>> GetAllCustomers(this SwDbContext db)
    {
        return db.Customers.Where(c => c.DelDateTime == null).ToListAsync();
    }

    #endregion

    #region Transaction

    public static Task<List<Transaction>> GetTransactionsByAddDateTimeRangeForKPCashReceipt(
        this SwDbContext db,
        DateTime cash_rcpt_beg_datetime,
        DateTime cash_rcpt_end_datetime,
        int customerId)
    {
        var codes = new[] { "KP", "PP" };

        return db.Transactions
            .Where(t => codes.Contains(t.TransactionCode.Code))
            .Where(t => t.CustomerId == customerId)
            .Where(t => t.AddDateTime >= cash_rcpt_beg_datetime && t.AddDateTime < cash_rcpt_end_datetime)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.Comment.Contains("KanPay"))
            //.Include(t => t.Customer)   // ?
            .Include(t => t.TransactionCode)
            .OrderBy(t => t.CustomerId)
            .ThenByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ToListAsync();
    }

    public static Task<List<Transaction>> GetTransactionsChargesForCustomer(this SwDbContext db, int customerId)
    {
        var codes = new[] { "MB", "FB", "LF", "CF", "WC", "WRC", "WRO", "WRR", "RF", "RD", "RR", "EP", "BK", "CR" };

        return db.Transactions
            .Where(t => codes.Contains(t.TransactionCode.Code))
            .Where(t => !t.DeleteFlag)
            .Where(t => t.PaidFull != true)
            .Where(t => t.CustomerId == customerId)
            //.Include(t => t.Customer)   // ?
            .Include(t => t.TransactionCode)
            .OrderBy(t => t.CustomerId)
            .ThenBy(t => t.AddDateTime)
            .ThenBy(t => t.Sequence)
            .ToListAsync();
    }

    public static Task<List<Transaction>> GetUpdatedChargeTransactionsByAddDateTimeRangeForCashReceipt(
        this SwDbContext db,
        DateTime cash_receipt_beg_datetime,
        DateTime cash_receipt_end_datetime)
    {
        return db.Transactions
            .Where(t =>
                t.ChgDateTime >= cash_receipt_beg_datetime &&
                t.ChgDateTime <= cash_receipt_end_datetime &&
                t.ChgToi == "Cash Receipt" &&
                !t.DeleteFlag)
            //.Include(t => t.Customer)   // ?
            .Include(t => t.TransactionCode)
            .ToListAsync();
    }

    #endregion
}
