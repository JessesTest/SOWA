using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SW.BLL.Extensions
{
    public static class DbContextExtensions
    {

        #region Container

        public static Task GetContainersByServiceAddress(this SwDbContext db, int serviceAddressId)
        {
            return db.Containers
                .Where(e => e.ServiceAddressId == serviceAddressId)
                .Where(e => !e.DeleteFlag)
                .Include(e => e.ServiceAddress)
                .Include(e => e.ContainerCode)
                .Include(e => e.ContainerSubtype)
                .Include(e => e.BillContainerDetails)
                .ToListAsync();
        }

        public static Task<Container> GetContainerById(this SwDbContext db, int containerId)
        {
            return db.Containers
                .Where(e => e.Id == containerId)
                //.Where(e => !e.DeleteFlag)
                .Include(e => e.ServiceAddress)
                .Include(e => e.ContainerCode)
                .Include(e => e.ContainerSubtype)
                .Include(e => e.BillContainerDetails)
                .SingleOrDefaultAsync();
        }

        #endregion

        #region Container Code

        public static ValueTask<ContainerCode> GetContainerCodeById(this SwDbContext db, int containerCodeId)
        {
            return db.ContainerCodes.FindAsync(containerCodeId);
        }

        #endregion

        #region Customer

        public static Task<Customer> GetCustomerById(this SwDbContext db,  int customerId)
        {
            return db.Customers
                .Where(e => e.CustomerId == customerId || e.LegacyCustomerId == customerId)
                .SingleOrDefaultAsync();
        }

        public static Task<Customer> GetCustomerByPe(this SwDbContext db, int pe)
        {
            return db.Customers
                .Where(e => e.Pe == pe)
                .SingleOrDefaultAsync();
        }

        #endregion

        #region Payment Plan

        public static Task<PaymentPlan> GetActivePaymentPlanByCustomer(this SwDbContext db, int customerId, bool includeDetails = true)
        {
            var query = db.PaymentPlans
                .Where(pp => pp.CustomerId == customerId)
                .Where(pp => pp.Customer.PaymentPlan)
                .Where(pp => !pp.DelFlag)
                .Where(pp => !pp.Canceled);

            if (includeDetails)
                query = query.Include(e => e.Details);

            return query
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        #endregion

        #region Service Address

        public static Task<List<ServiceAddress>> GetServiceAddressByCustomer(this SwDbContext db, int customerId)
        {
            return db.ServiceAddresses
                .Where(s => s.CustomerId == customerId && !s.DeleteFlag)
                .Include(s => s.Customer)
                .Include(s => s.Containers)
                .Include(s => s.ServiceAddressNotes)
                .Include(s => s.BillServiceAddresses)
                .AsSplitQuery()
                .ToListAsync();
        }

        public static Task<ServiceAddress> GetServiceAddressById(this SwDbContext db, int serviceAddressId)
        {
            return db.ServiceAddresses
                .Where(s => s.Id == serviceAddressId)
                .Include(s => s.Customer)
                .Include(s => s.Containers)
                .Include(s => s.ServiceAddressNotes)
                .Include(s => s.BillServiceAddresses)
                .AsSplitQuery()
                .SingleOrDefaultAsync();
        }

        #endregion

        #region Transaction Code

        public static ValueTask<TransactionCode> GetTransactionCode(this SwDbContext db, int transactionCodeId)
        {
            return db.TransactionCodes.FindAsync(transactionCodeId);
        }

        #endregion

    }
}
