using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using System.Linq;

namespace SW.BLL.Services;

public class BillingSummaryService : IBillingSummaryService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly IAddressService addressService;

    public BillingSummaryService(
        IDbContextFactory<SwDbContext> dbFactory,
        IAddressService addressService)
    {
        this.dbFactory = dbFactory;
        this.addressService = addressService;
    }

    public async Task<decimal> GetTotalPaymentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate)
    {
        string[] paymentCodes = { "CC", "KP", "P", "BW", "PP", "IP", "JE", "ACH" };

        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);

        return await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime >= startDate)
            .Where(t => t.AddDateTime <= endDate)
            .Where(t => paymentCodes.Contains(t.TransactionCode.Code))
            .SumAsync(t => t.TransactionAmt);
    }

    public async Task<decimal> GetTotalAdjustmentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate)
    {
        string[] paymentCodes = { "BD", "R", "C", "DER", "DEM", "RC", "DEP", "DEC", "DEK", "B" };

        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);

        return await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime >= startDate)
            .Where(t => t.AddDateTime <= endDate)
            .Where(t => paymentCodes.Contains(t.TransactionCode.Code))
            .SumAsync(t => t.TransactionAmt);
    }

    public async Task<decimal> GetTotalNewChargesForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate)
    {
        string[] paymentCodes = { "BK", "EP", "MB", "RD", "RR", "MBR", "FB", "LF", "CF", "WC", "WRC", "WRO", "WRR", "CR" };

        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);

        return await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime >= startDate)
            .Where(t => t.AddDateTime <= endDate)
            .Where(t => paymentCodes.Contains(t.TransactionCode.Code))
            .SumAsync(t => t.TransactionAmt);
    }

    public Task<BillingSummary> GetBillingSummary(int customerId)
    {
        return GetBillingSummary(customerId, DateTime.Now);
    }

    public async Task<BillingSummary> GetBillingSummary(int customerId, DateTime onDate)
    {
        onDate = onDate.Date;

        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);
        if (customer == null)
            throw new ArgumentException("Customer not found", nameof(customerId));

        BillingSummary bs = new();
        bs.SetContractCharge(customer.ContractCharge);

        var serviceAddresses = await db.ServiceAddresses
            .Where(sa => sa.CustomerId == customer.CustomerId && sa.CustomerType == customer.CustomerType && !sa.DeleteFlag && (!sa.CancelDate.HasValue || sa.CancelDate.Value >= onDate))
            .Include(sa => sa.Containers.Where(c => !c.DeleteFlag && (!c.CancelDate.HasValue || c.CancelDate.Value >= onDate)))
            .ThenInclude(c => c.ContainerCode)
            .ToListAsync();

        var peAddressIds = serviceAddresses.Select(e => e.PeaddressId).ToList();
        var peAddresses = await addressService.GetByIds(peAddressIds);
        foreach(var a in serviceAddresses)
        {
            var bsa = new BillingSummaryServiceAddress
            {
                ServiceAddress = a,
                Address = peAddresses.SingleOrDefault(e => e.Id == a.PeaddressId)
            };
            bs.Add(bsa);

            foreach(var c in a.Containers)
            {
                BillingSummaryContainer bsc = new()
                {
                    Container = c
                };

                var effective_date = DateTime.Today;
                if (c.EffectiveDate > DateTime.Today)
                {
                    effective_date = c.EffectiveDate;
                }

                bsc.Rate = await db.ContainerRates
                    .Where(r => 
                        r.BillingSize == c.BillingSize &&
                        r.ContainerType == c.ContainerCodeId &&
                        r.EffectiveDate <= effective_date &&
                        r.NumDaysService == c.NumDaysService &&
                        !r.DeleteFlag &&
                        r.ContainerSubtypeId == c.ContainerSubtypeId)
                    .OrderByDescending(r => r.EffectiveDate)
                    .FirstOrDefaultAsync();

                bsa.Add(bsc);
            }
        }
        return bs;
    }

    public Task<BillingSummary> GetBillingSummaryForPaymentPlan(int customerId)
    {
        return GetBillingSummaryForPaymentPlan(customerId, DateTime.Now);
    }
    internal async Task<BillingSummary> GetBillingSummaryForPaymentPlan(int customerId, DateTime onDate)
    {
        using var db = dbFactory.CreateDbContext();

        onDate = onDate.Date;
        var customer = await db.GetCustomerById(customerId);
        if (customer == null)
        {
            throw new ArgumentException("Customer not found", nameof(customerId));
        }

        //var pe = await personEntityService.GetById(customer.Pe)

        var bs = new BillingSummary();
        bs.SetContractCharge(customer.ContractCharge);

        var addresses = await db.ServiceAddresses.Where(a =>
            a.CustomerId == customer.CustomerId &&
            a.CustomerType == customer.CustomerType &&
            !a.DeleteFlag &&
            a.EffectiveDate <= onDate &&
            (!a.CancelDate.HasValue || a.CancelDate.Value >= onDate))
            .ToListAsync();
        foreach (var a in addresses)
        {
            var bsa = new BillingSummaryServiceAddress
            {
                ServiceAddress = a,
                Address = await addressService.GetById(a.PeaddressId)
            };
            bs.Add(bsa);

            //SCMB-243-New-Container-Rates-For-2022
            var containers = await db.Containers
                .Where(c => c.ServiceAddressId == a.Id && !c.DeleteFlag && c.EffectiveDate <= onDate && (!c.CancelDate.HasValue || c.CancelDate.Value >= onDate))
                .Include(c => c.ContainerCode)
                .ToListAsync();

            foreach (var c in containers)
            {
                int? daysOfService = c.NumDaysService;
                BillingSummaryContainer bsc = new()
                {
                    Container = c
                };

                bsc.Rate = await db.ContainerRates.Where(r =>
                    r.BillingSize == c.BillingSize &&
                    r.ContainerType == c.ContainerCodeId &&
                    //r.EffectiveDate <= effective_date &&
                    r.EffectiveDate <= DateTime.Now &&
                    r.NumDaysService == daysOfService &&
                    !r.DeleteFlag &&
                    r.ContainerSubtypeId == c.ContainerSubtypeId)
                    .OrderByDescending(r => r.EffectiveDate)
                    .FirstOrDefaultAsync();
                bsa.Add(bsc);
            }
        }

        return bs;

    }
}
