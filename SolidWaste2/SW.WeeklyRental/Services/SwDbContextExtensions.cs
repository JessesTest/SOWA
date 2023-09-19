using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.WeeklyRental.Services;

public static class SwDbContextExtensions
{
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
            .Where(r => r.ContainerType == containerCodeId)
            .Where(r => r.ContainerSubtypeId == containerSubtypeID)
            .Where(r => r.NumDaysService == dayCount)
            .Where(r => r.BillingSize == billingSize)
            .Where(r => r.EffectiveDate <= effectiveDate)
            .Where(r => !r.DeleteFlag)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Customer

    public static Task<List<Customer>> GetCustomers(this SwDbContext db)
    {
        return db.Customers
            .ToListAsync();
    }

    #endregion

    #region ServiceAddress

    public static Task<List<ServiceAddress>> GetServiceAddressesByCustomer(this SwDbContext db, int customerId)
    {
        return db.ServiceAddresses
            .Where(s => s.CustomerId == customerId && !s.DeleteFlag)
            //.Include(s => s.Customer)
            .Include(s => s.Containers.Where(c => !c.DeleteFlag)) 
            .ThenInclude(s => s.ContainerCode)
            //.Include(s => s.ServiceAddressNotes)    // ?
            //.Include(s => s.BillServiceAddresses)   // ?
            .OrderBy(s => s.LocationNumber)
            .ToListAsync();
    }

    #endregion

    #region Transaction

    //public static Task<List<Transaction>> GetTransactionsByCustomer(this SwDbContext db, int customerId)
    //{
    //    return db.Transactions
    //        .Where(t => t.CustomerId == customerId)
    //        .OrderByDescending(t => t.AddDateTime)
    //        .ThenByDescending(t => t.Sequence)
    //        .ToListAsync();
    //}
    public static Task<Transaction> GetLastTransaction(this SwDbContext db, int customerId)
    {
        return db.Transactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .FirstOrDefaultAsync();
    }
    //public static async Task<decimal> GetBalanceForward(this SwDbContext db, int customerId)
    //{
    //    var transaction = await db.GetLastTransaction(customerId);
    //    return transaction?.TransactionBalance ?? 0.00m;
    //}

    #endregion

    #region TransactionCode

    public static Task<List<TransactionCode>> GetTransactionCodes(this SwDbContext db)
    {
        var codes = new[] { "WRC", "WRO", "WRR" };

        return db.TransactionCodes
            .Where(c => codes.Contains(c.Code))
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    #endregion





    #region Container

    public static Task<List<Container>> GetContainers(this SwDbContext db, DateTime today)
    {
        var mSubtypes = new[] { 7, 8, 9 };

        return db.Containers
            .Where(c => !c.DeleteFlag)
            .Where(c => c.CancelDate == null || c.CancelDate >= today)
            .Where(c => c.EffectiveDate < today)
            .Where(c =>
                (c.ContainerCode.Type == "C" && c.ContainerSubtypeId == 2) ||
                (c.ContainerCode.Type == "O") ||
                (c.ContainerCode.Type == "M" && mSubtypes.Contains(c.ContainerSubtypeId))
            )

            .Where(c => !c.ServiceAddress.DeleteFlag)
            .Where(c => c.ServiceAddress.CancelDate == null)

            .Where(c => c.ServiceAddress.Customer.ContractCharge == null)
            .Where(c => c.ServiceAddress.Customer.CancelDate == null)

            .Include(c => c.ContainerCode)
            .Include(c => c.ServiceAddress)

            .OrderBy(c => c.ServiceAddress.CustomerId)
            .ThenBy(c => c.ServiceAddress.LocationNumber)
            .ThenBy(c => c.EffectiveDate)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    #endregion

}
