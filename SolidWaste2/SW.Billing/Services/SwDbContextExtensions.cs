using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SW.Billing.Services;

public static class SwDbContextExtensions
{
    #region Customers

    public static Task<List<Customer>> GetAllCustomers(this SwDbContext db) 
    {
        return db.Customers.ToListAsync();
    }

    #endregion

    #region Transaction Codes

    public static Task<List<TransactionCode>> GetBillingTransactionCodes(this SwDbContext db)
    {
        return db.TransactionCodes
            .Where(t => !t.DeleteFlag && (t.Code == "MB" || t.Code == "MBR" || t.Code == "FB"))
            .OrderBy(t => t.Code)
            .ToListAsync();
    }

    #endregion

    #region Transactions

    public static Task<List<Transaction>> GetLatestMonthlyBillingTransactions(this SwDbContext db, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        string comment_1 = string.Concat(mthly_bill_beg_datetime.ToString("MMM").ToUpper(), " ", mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0'), " ", "MONTHLY BILL");
        string comment_2 = string.Concat(mthly_bill_beg_datetime.ToString("MMM").ToUpper(), " ", mthly_bill_beg_datetime.Year.ToString().Substring(2, 2).PadLeft(2, '0'), " ", "MONTHLY BILL");

        return db.Transactions
            .Include(t => t.TransactionCode)
            .Where(t => t.TransactionCodeId == t.TransactionCode.TransactionCodeId && (t.TransactionCode.Code == "MB" || t.TransactionCode.Code == "MBR" || t.TransactionCode.Code == "FB") && t.AddDateTime > mthly_bill_end_datetime && (t.Comment == comment_1 || t.Comment == comment_2) && !t.DeleteFlag)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ToListAsync();
    }

    public static Task<List<Transaction>> GetTransactionsByCustomerId(this SwDbContext db, int customer_id)
    {
        return db.Transactions
            .Include(t => t.TransactionCode)
            .Include(t => t.Customer)
            .Where(t => t.CustomerId == customer_id && !t.DeleteFlag)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ToListAsync();
    }

    public static Task<decimal> GetRemainingCurrentBalance(this SwDbContext db, DateTime date, int days, int customer_id) 
    {
        var endDate = date.Date.AddDays(1);
        var startDate = date.Date.AddDays(-days);

        string[] billTypes = { "MB", "FB", "MBR" };

        Transaction bill = db.Transactions
            .Where(t => t.CustomerId == customer_id && !t.DeleteFlag && billTypes.Contains(t.TransactionCode.Code) && t.AddDateTime < startDate)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .FirstOrDefault();

        if (bill == null || bill.TransactionBalance <= 0) 
        {
            return Task.FromResult(0m);
        }

        var transactions = db.Transactions
            .Where(t => t.CustomerId == customer_id && !t.DeleteFlag && t.AddDateTime < endDate && t.TransactionAmt < 0 && (t.AddDateTime > bill.AddDateTime || (t.AddDateTime == bill.AddDateTime && t.Sequence > bill.Sequence)))
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ToList();

        var total = bill.TransactionBalance + transactions.Select(t => t.TransactionAmt).Sum();

        if (total < 0) 
        {
            return Task.FromResult(0m);
        }

        return Task.FromResult(total);
    }

    public static Task<List<Transaction>> GetTransactionsByAddDateTimeRange(this SwDbContext db, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        var codes = new[] { "MB", "CF", "WC", "WRC", "WRO", "WRR", "RF", "RD", "RR", "EP", "BK", "CR" };

        return db.Transactions
            .Include(t => t.Customer)
            .Include(t => t.TransactionCode)
            .Where(t => codes.Contains(t.TransactionCode.Code) && t.AddDateTime >= mthly_bill_beg_datetime && !t.DeleteFlag)
            .ToListAsync();
            
    }

    #endregion

    #region Service Address

    public static Task<List<ServiceAddress>> GetServiceAddressByCustomer(this SwDbContext db, int customer_id)
    {
        return db.ServiceAddresses
            .Include(s => s.Customer)
            .Include(s => s.Containers)
            .Include(s => s.ServiceAddressNotes)
            .Include(s => s.BillServiceAddresses)
            .Where(s => s.CustomerId == customer_id && !s.DeleteFlag)
            .OrderBy(s => s.LocationNumber)
            .ToListAsync();
    }

    #endregion

    #region Container Codes

    public static Task<ContainerCode> GetContainerCodeById(this SwDbContext db, int id)
    {
        return db.ContainerCodes.Where(c => c.ContainerCodeId == id).SingleAsync();
    }

    #endregion

    #region Container Subtypes

    public static Task<ContainerSubtype> GetContainerSubtypeById(this SwDbContext db, int containerSubtypeId)
    {
        return db.ContainerSubtypes.Where(c => c.ContainerSubtypeId == containerSubtypeId).SingleAsync();
    }

    #endregion

    #region Container Rates

    public static Task<List<ContainerRate>> GetContainerRateByCodeDaysSizeEffDate(this SwDbContext db, int containerCodeId, int containerSubtypeID, int dayCount, decimal billingSize, DateTime effectiveDate)
    {
        return db.ContainerRates
            .Where(c => c.ContainerType == containerCodeId && c.ContainerSubtypeId == containerSubtypeID && c.NumDaysService == dayCount && c.BillingSize == billingSize && c.EffectiveDate <= effectiveDate && !c.DeleteFlag)
            .OrderByDescending(t => t.EffectiveDate)
            .ToListAsync();
    }

    #endregion

    #region Containers

    public static Task<List<Container>> GetContainersByServiceAddress(this SwDbContext db, int serviceAddressId)
    {
        return db.Containers
            .Include(c => c.ServiceAddress)
            .Include(s => s.ContainerCode)
            .Include(s => s.ContainerSubtype)
            .Include(s => s.BillContainerDetails)
            .Where(s => s.ServiceAddressId == serviceAddressId && !s.DeleteFlag)
            .ToListAsync();
    }

    #endregion
}
