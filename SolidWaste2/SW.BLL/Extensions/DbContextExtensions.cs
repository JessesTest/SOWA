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

        #region Transaction

        public static async Task<ICollection<Transaction>> GetDelinquencyFeesToPay(this SwDbContext db, int customerId, TransactionCode code)
        {
            ICollection<Transaction> feesToPay;

            if (code.IsCollections)
            {
                // collections
                feesToPay = await db.Transactions
                    .Where(t => !t.DeleteFlag && t.CustomerId == customerId && t.CollectionsAmount > 0 && (!t.PaidFull.HasValue || !t.PaidFull.Value))
                    .ToListAsync();
            }
            else if (code.IsCounselors)
            {
                // counselors
                feesToPay = await db.Transactions
                    .Where(t => !t.DeleteFlag && t.CustomerId == customerId && t.CounselorsAmount > 0 && (!t.PaidFull.HasValue || !t.PaidFull.Value))
                    .ToListAsync();
            }
            else if (code.IsUncollectable)
            {
                // uncollectable
                feesToPay = await db.Transactions
                    .Where(t => !t.DeleteFlag && t.CustomerId == customerId && t.UncollectableAmount > 0 && (!t.PaidFull.HasValue || !t.PaidFull.Value))
                    .ToListAsync();
            }
            else
            {
                return new List<Transaction>();
            }
            return feesToPay.OrderBy(t => t.AddDateTime).ThenBy(t => t.Sequence).ThenBy(t => t.Id).ToList();
        }

        public static async Task<Transaction> GetLatesetTransaction(this SwDbContext db, int customerId)
        {
            var customer = await db.GetCustomerById(customerId);

            return await db.Transactions
                .Where(t => t.CustomerId == customer.CustomerId)
                .Where(t => !t.DeleteFlag)
                .OrderByDescending(t => t.AddDateTime)
                .ThenByDescending(t => t.Sequence)
                .ThenByDescending(t => t.Id)
                .FirstOrDefaultAsync();
        }

        public static async Task AddTransaction(this SwDbContext db, Transaction transaction)
        {
            var customerId = transaction.CustomerId;
            var customer = await db.Customers
                .Where(e => e.CustomerId == customerId || e.LegacyCustomerId == customerId)
                .Where(e => e.DelDateTime == null)
                .SingleAsync();

            var lastTransaction = await db.Transactions
                .Where(t => t.CustomerId == customer.CustomerId)
                .Where(t => !t.DeleteFlag)
                .OrderByDescending(t => t.Sequence)
                .OrderByDescending(t => t.AddDateTime)
                .FirstOrDefaultAsync();

            if (lastTransaction != null && transaction.AddDateTime.ToString("G") == lastTransaction.AddDateTime.ToString("G"))
            {
                transaction.Sequence = lastTransaction.Sequence + 1;
            }

            db.Transactions.Add(transaction);
        }

        public static void UpdateTransaction(this SwDbContext db, Transaction transaction)
        {
            transaction.ChgDateTime = DateTime.Now;
            db.Transactions.Update(transaction);
        }

        #endregion

        #region Transaction Code

        public static ValueTask<TransactionCode> GetTransactionCode(this SwDbContext db, int transactionCodeId)
        {
            return db.TransactionCodes.FindAsync(transactionCodeId);
        }

        public static Task<TransactionCode> GetTransactionCodeByCode(this SwDbContext db, string code)
        {
            return db.TransactionCodes.Where(c => c.Code == code && !c.DeleteFlag).SingleOrDefaultAsync();
        }

        #endregion

    }
}
