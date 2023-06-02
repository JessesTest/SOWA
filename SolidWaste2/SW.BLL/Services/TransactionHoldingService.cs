using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionHoldingService : ITransactionHoldingService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly int cutoffDays = 30;

    public TransactionHoldingService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ICollection<TransactionHolding>> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionHoldings
            .Where(th => !th.DeleteFlag)
            .Include(th => th.TransactionCode)
            .OrderByDescending(th => th.AddDateTime)
            .ToListAsync();
    }

    public async Task<ICollection<TransactionHolding>> GetAllAuthorized(string email)
    {
        using var db = dbFactory.CreateDbContext();
        IQueryable<TransactionHolding> query = db.TransactionHoldings
            .Where(th => !th.DeleteFlag);

        query = string.IsNullOrWhiteSpace(email) ?
            query.Where(th => th.Security == null) :
            query.Where(th => th.Security == null || th.Security == email);

        var approvedStatuses = new[] { "Resolved", "Approved", "Rejected" };
        var cutoffDate = DateTime.Today.AddDays(-cutoffDays);
        query = query
            .Where(th => th.ChgDateTime == null || (approvedStatuses.Contains(th.Status) && th.ChgDateTime >= cutoffDate));

        return await query
            .Include(th => th.TransactionCode)
            .OrderByDescending(th => th.AddDateTime)
            .ToListAsync();
    }

    public async Task<TransactionHolding> GetAuthorizedById(string email, int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await GetAuthorized(db, email, id);
    }

    private async Task<TransactionHolding> GetAuthorized(SwDbContext db, string email, int id)
    {
        IQueryable<TransactionHolding> query = db.TransactionHoldings
            .Where(th => th.TransactionHoldingId == id)
            .Where(th => !th.DeleteFlag);

        query = string.IsNullOrWhiteSpace(email) ?
            query.Where(th => th.Security == null) :
            query.Where(th => th.Security == null || th.Security == email);

        var approvedStatuses = new[] { "Resolved", "Approved", "Rejected" };
        var cutoffDate = DateTime.Today.AddDays(-cutoffDays);
        query = query
            .Where(th => th.ChgDateTime == null || (approvedStatuses.Contains(th.Status) && th.ChgDateTime >= cutoffDate));

        return await query
            .Include(th => th.TransactionCode)
            .OrderByDescending(th => th.AddDateTime)
            .SingleOrDefaultAsync();
    }

    #region Resolve/Approve/Reject

    public async Task<string> Resolve(int transactionHoldingId, string email, string displayName)
    {
        Transaction associatedTransaction = null;
        string result;

        using var db = dbFactory.CreateDbContext();
        var transactionHolding = await GetAuthorized(db, email.ToUpper(), transactionHoldingId);

        result = Resolve_ValidateTransactionHolding(transactionHolding, email);
        if (result != null)
            return result;

        // If an associated transaction is provided, make sure it is valid
        if (transactionHolding.AssociatedTransactionId.HasValue)
        {
            associatedTransaction = await db.Transactions
                .Where(t => t.Id == transactionHolding.AssociatedTransactionId)
                .Include(t => t.TransactionHolding)
                .FirstOrDefaultAsync();

            result = Resolve_ValidateAssociatedTransaction(transactionHolding, associatedTransaction);
            if (result != null)
                return result;
        }

        // ???

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == transactionHolding.CustomerId);

        Transaction transaction = new ()
        {
            CustomerId = customer.CustomerId,
            CustomerType = customer.CustomerType,
            TransactionCodeId = transactionHolding.TransactionCodeId,
            TransactionAmt = transactionHolding.TransactionAmt,
            CheckNumber = transactionHolding.CheckNumber,
            Comment = transactionHolding.Comment,
            WorkOrder = transactionHolding.WorkOrder,
            TransactionHoldingId = transactionHolding.TransactionHoldingId,
            ServiceAddressId = transactionHolding.ServiceAddressId,
            ContainerId = transactionHolding.ContainerId,
            ObjectCode = transactionHolding.ObjectCode,
            AssociatedTransactionId = transactionHolding.AssociatedTransactionId,
            AddDateTime = DateTime.Now,
            AddToi = displayName,
        };

        if (transactionHolding.BatchId > 0)
        {
            transactionHolding.Sender = string.Concat(transactionHolding.Sender, " ", transactionHolding.BatchId);
        }

        var dateTime = DateTime.Now;
        result = await AddTransactionAutoCalc(db, transaction);
        if (result != null)
            return result;

        if (associatedTransaction != null)
        {
            associatedTransaction.Partial = associatedTransaction.TransactionAmt;
            associatedTransaction.PaidFull = true;
            associatedTransaction.ChgToi = displayName;
            associatedTransaction.ChgDateTime = dateTime;
        }

        transactionHolding.Status = "Resolved";
        transactionHolding.Approver = email;
        transactionHolding.ChgToi = displayName;
        transactionHolding.ChgDateTime = dateTime;

        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string> Approve(int transactionHoldingId, string email, string displayName)
    {
        Transaction associatedTransaction = null;
        string result;
        using var db = dbFactory.CreateDbContext();
        var transactionHolding = await GetAuthorized(db, email.ToUpper(), transactionHoldingId);

        result = Resolve_ValidateTransactionHolding(transactionHolding, email);
        if (result != null)
            return result;

        // If an associated transaction is provided, make sure it is valid
        if (transactionHolding.AssociatedTransactionId.HasValue)
        {
            associatedTransaction = await db.Transactions
                .Where(t => t.Id == transactionHolding.AssociatedTransactionId)
                .Include(t => t.TransactionHolding)
                .FirstOrDefaultAsync();

            result = Resolve_ValidateAssociatedTransaction(transactionHolding, associatedTransaction);
            if (result != null)
                return result;
        }

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == transactionHolding.CustomerId);

        Transaction transaction = new()
        {
            CustomerId = customer.CustomerId,
            CustomerType = customer.CustomerType,
            TransactionCodeId = transactionHolding.TransactionCodeId,
            TransactionAmt = transactionHolding.TransactionAmt,
            CheckNumber = transactionHolding.CheckNumber,
            Comment = transactionHolding.Comment,
            WorkOrder = transactionHolding.WorkOrder,
            TransactionHoldingId = transactionHolding.TransactionHoldingId,
            ServiceAddressId = transactionHolding.ServiceAddressId,
            ContainerId = transactionHolding.ContainerId,
            ObjectCode = transactionHolding.ObjectCode,
            AssociatedTransactionId = transactionHolding.AssociatedTransactionId,
            AddDateTime = DateTime.Now,
            AddToi = displayName
        };

        result = await AddTransactionAutoCalc(db, transaction);
        if (result != null)
            return result;

        if (associatedTransaction != null)
        {
            associatedTransaction.Partial = associatedTransaction.TransactionAmt;
            associatedTransaction.PaidFull = true;
            associatedTransaction.ChgToi = displayName;
            associatedTransaction.ChgDateTime = DateTime.Now;
        }

        transactionHolding.Status = "Approved";
        transactionHolding.Approver = email;
        transactionHolding.ChgToi = displayName;
        transactionHolding.ChgDateTime = DateTime.Now;

        await db.SaveChangesAsync();
        return null;
    }

    private static string Resolve_ValidateTransactionHolding(TransactionHolding transactionHolding, string email)
    {
        // Make sure the transactionHolding exists
        if (transactionHolding == null)
            return "Transaction Holding item does not exist";

        // Make sure the transactionHolding is not deleted
        if (transactionHolding.DeleteFlag)
            return "Transaction Holding item has been previously deleted";

        // Make sure the transactionHolding is not already resolved
        if (transactionHolding.Status == "Resolved")
            return "Transaction Holding item has already been resolved";

        // Make sure the transactionHolding is not already approved
        if (transactionHolding.Status == "Approved")
            return "Transaction Holding item has already been aproved";

        // Make sure the transactionHolding is not already rejected
        if (transactionHolding.Status == "Rejected")
            return "TransactionHolding item has already been rejected";

        // Make sure the user is not trying to self-approve
        if (transactionHolding.Sender.ToUpper().Trim() == email.ToUpper().Trim())
            return "Cannot self-approve Transaction Holding";

        return null;
    }

    private static string Resolve_ValidateAssociatedTransaction(TransactionHolding transactionHolding, Transaction associatedTransaction)
    {
        // Make sure transaction exists
        if (associatedTransaction == null || associatedTransaction.DeleteFlag)
            return "Invalid associated transaction id";

        // Make sure associated transaction is not already paid off
        if (associatedTransaction.PaidFull.HasValue && associatedTransaction.PaidFull.Value)
            return "Associated transaction already paid in full";

        // Make sure associated transaction is of a valid transaction type
        string[] associatedTransactionTypes = { "LF" };
        if (!associatedTransactionTypes.Contains(associatedTransaction.TransactionCode.Code))
            return "Associated transaction not a valid transaction type";

        // Make sure associated transaction value matches transaction value
        if (associatedTransaction.TransactionAmt + transactionHolding.TransactionAmt != 0)
            return "TransactionHolding amount does not match associated transaction amount";

        return null;
    }

    public async Task<string> Reject(int transactionHoldingId, string comment, string email, string displayName)
    {
        using var db = dbFactory.CreateDbContext();
        var transactionHolding = await GetAuthorized(db, email.ToUpper(), transactionHoldingId);

        var result = Resolve_ValidateTransactionHolding(transactionHolding, email);
        if (result != null)
            return result;

        transactionHolding.Status = "Rejected";
        transactionHolding.Approver = email;
        transactionHolding.ChgToi = displayName;
        transactionHolding.ChgDateTime = DateTime.Now;

        await db.SaveChangesAsync();
        return null;
    }

    private static async Task<string> AddTransactionAutoCalc(SwDbContext db, Transaction transaction)
    {
        // Make sure there is an add toi
        if (string.IsNullOrWhiteSpace(transaction.AddToi))
            return "No user defined";

        // Make sure there is a valid customer id
        var customer = await db.Customers.FindAsync(transaction.CustomerType,transaction.CustomerId);
        if (customer == null)
            return "Invalid customer id";

        // Make sure there is a valid transaction code
        var transactionCode = await db.TransactionCodes.FindAsync(transaction.TransactionCodeId.Value);
        if (transactionCode == null)
            return "Invalid transaction code";

        //// If an associated transaction is provided, make sure it is valid
        //if (transaction.AssociatedTransactionId.HasValue && !TransactionExists(transaction.AssociatedTransactionId.Value))
        //    return "Invalid associated transaction id"

        // Make sure the transaction amount has the right sign
        if (transactionCode.TransactionSign == "P" && transaction.TransactionAmt < 0)
            transaction.TransactionAmt *= -1;
        if (transactionCode.TransactionSign == "N" && transaction.TransactionAmt > 0)
            transaction.TransactionAmt *= -1;

        // Is this a counselor payment?
        if (transactionCode.Code == "DEC")
        {
            transaction.CounselorsAmount = transaction.TransactionAmt;
            transaction.TransactionAmt = 0;
        }

        // Is this a collection payment?
        if (transactionCode.Code == "BW" || transactionCode.Code == "DEK")
        {
            transaction.CollectionsAmount = transaction.TransactionAmt;
            transaction.TransactionAmt = 0;
        }

        // Make sure there is a sequence number
        if (transaction.Sequence <= 0)
            transaction.Sequence = 1;

        // Set the initial transaction balance
        var lastTransaction = await GetLatestTransaction(db, customer.CustomerId);

        if (transactionCode.Group == "P" && lastTransaction != null && lastTransaction.CounselorsBalance > 0)
        {
            var counselorsAmount = 0m;
            var transactionAmount = -transaction.TransactionAmt;

            if (lastTransaction.CounselorsBalance >= transactionAmount)
            {
                counselorsAmount = transactionAmount;
                transactionAmount = 0;
            }
            else
            {
                counselorsAmount = lastTransaction.CounselorsBalance;
                transactionAmount = transactionAmount - counselorsAmount;
            }

            transaction.TransactionAmt = -transactionAmount;

            if (counselorsAmount > 0)
            {
                var result = await MakeDelinquencyPayment(
                    db,
                    customer.CustomerId,
                    customer.CustomerType,
                    "PCC",
                    counselorsAmount,
                    transaction.Comment,
                     DateTime.Now);
                if (result != null)
                    return result;
            }

            if (transaction.TransactionAmt == 0)
                return null;

            lastTransaction = await GetLatestTransaction(db, customer.CustomerId);
        }

        if (lastTransaction == null)
        {
            transaction.TransactionBalance = 0;
            transaction.CounselorsBalance = 0;
            transaction.CollectionsBalance = 0;
            transaction.UncollectableBalance = 0;
        }
        else
        {
            transaction.TransactionBalance = lastTransaction.TransactionBalance;
            transaction.CounselorsBalance = lastTransaction.CounselorsBalance;
            transaction.CollectionsBalance = lastTransaction.CollectionsBalance;
            transaction.UncollectableBalance = lastTransaction.UncollectableBalance ?? 0;
        }

        // Clear out Counselor's Balance first
        if (lastTransaction != null && lastTransaction.CounselorsBalance > 0 && transaction.TransactionAmt < 0)
        {
            decimal amtNeg = transaction.TransactionAmt;

            if (transactionCode.TransactionSign == "N")
                amtNeg *= -1;

            if (amtNeg >= transaction.CounselorsBalance)
            {
                transaction.TransactionAmt += transaction.CounselorsBalance;
                transaction.CounselorsAmount = -transaction.CounselorsBalance;
            }
            else
            {
                transaction.CounselorsAmount = transaction.TransactionAmt;
                transaction.TransactionAmt = 0;
            }
        }

        if (transaction.UncollectableAmount == null)
        {
            transaction.UncollectableAmount = 0;
        }

        // Add the new transaction amount to the balance
        transaction.TransactionBalance += transaction.TransactionAmt;
        transaction.CounselorsBalance += transaction.CounselorsAmount;
        transaction.CollectionsBalance += transaction.CollectionsAmount;
        transaction.UncollectableBalance += transaction.UncollectableAmount;

        // "Pay off" payment plans if customer is on one
        var paymentPlan = await GetActivePaymentPlanByCustomer(db, customer.CustomerId);

        if (transactionCode.Code == "PP" && paymentPlan != null)
        {
            if (transaction.TransactionBalance <= 0)
            {
                paymentPlan.Canceled = true;
                //UpdatePaymentPlan(paymentPlan)

                customer.PaymentPlan = false;
                //UpdateCustomer(customer)
            }
            else
            {
                var remainingBalance = -transaction.TransactionAmt;

                var firstUnpaidPaymentPlanDetail = paymentPlan.Details.FirstOrDefault(m => !m.Paid.GetValueOrDefault());

                if (firstUnpaidPaymentPlanDetail != null)
                {
                    var remainingTotal = firstUnpaidPaymentPlanDetail.PaymentTotal;
                    var remainingAmount = firstUnpaidPaymentPlanDetail.Amount;

                    if (remainingBalance >= remainingTotal)
                    {
                        remainingBalance -= remainingTotal;
                        remainingTotal = 0;
                        remainingAmount = 0;
                    }
                    else if (remainingBalance >= remainingAmount)
                    {
                        remainingTotal = firstUnpaidPaymentPlanDetail.PaymentTotal - remainingBalance;
                        remainingAmount = 0;
                        remainingBalance = 0;
                    }
                    else
                    {
                        remainingTotal = firstUnpaidPaymentPlanDetail.PaymentTotal - remainingBalance;
                        remainingAmount = firstUnpaidPaymentPlanDetail.Amount - remainingBalance;
                        remainingBalance = 0;
                    }

                    remainingBalance -= remainingTotal;

                    firstUnpaidPaymentPlanDetail.PaymentTotal = 0;
                    firstUnpaidPaymentPlanDetail.Amount = 0;
                    firstUnpaidPaymentPlanDetail.Paid = true;
                    //UpdatePaymentPlanDetail(firstUnpaidPaymentPlanDetail)
                }

                if (remainingBalance != 0)
                {
                    var unpaidPaymentPlanDetails = paymentPlan.Details.Where(m => !m.Paid.GetValueOrDefault());
                    var count = unpaidPaymentPlanDetails.Count();

                    foreach (var item in unpaidPaymentPlanDetails)
                    {
                        item.PaymentTotal -= remainingBalance / count;
                        item.Amount -= remainingBalance / count;
                        //UpdatePaymentPlanDetail(item)
                    }
                }
            }

            if (!paymentPlan.Details.Any(m => !m.Paid.GetValueOrDefault()))
            {
                paymentPlan.Canceled = true;
                //UpdatePaymentPlan(paymentPlan)

                customer.PaymentPlan = false;
                //UpdateCustomer(customer)
            }
        }

        transaction.AddDateTime = /*dateTime.HasValue ? dateTime.Value :*/ DateTime.Now;

        if (lastTransaction != null && transaction.AddDateTime.ToString("G") == lastTransaction.AddDateTime.ToString("G"))
            transaction.Sequence = lastTransaction.Sequence + 1;

        db.Transactions.Add(transaction);
        return null;
    }

    private static async Task<PaymentPlan> GetActivePaymentPlanByCustomer(SwDbContext db, int customerId)
    {
        var now = DateTime.Now;
        return await db.PaymentPlans
            .Where(pp => pp.CustomerId == customerId)
            .Where(pp => !pp.DelFlag && !pp.Canceled)
            .Where(pp => pp.Details.OrderByDescending(d => d.Id).First().DueDate < now)
            .SingleOrDefaultAsync();
    }

    private static async Task<string> MakeDelinquencyPayment(
        SwDbContext db, 
        int customerId,
        string customerType,
        string transactionTypeCode,
        decimal amount,
        string comment,
        DateTime? dateTime = null)
    {
        var customer = await db.Customers.FindAsync(customerType, customerId);
        if (customer == null || customer.DelDateTime.HasValue)
            return string.Format("Customer {0} not found", customerId);

        var code = await db.TransactionCodes.FindAsync(transactionTypeCode);
        if (code == null || code.DeleteFlag)
            return string.Format("Transaction code {0} not found", transactionTypeCode);

        Func<Transaction, decimal> GetTranAmt;
        if (code.IsCollections)
            GetTranAmt = t => t.CollectionsAmount;
        else if (code.IsCounselors)
            GetTranAmt = t => t.CounselorsAmount;
        else if (code.IsUncollectable)
            GetTranAmt = t => t.UncollectableAmount.GetValueOrDefault();
        else
            return "invalid transaction type";

        var feesToPay = await GetDelinquencyFeesToPay(db, customerId, code);
        var lastTransaction = await GetLatestTransaction(db, customerId);
        var sequence = 0;
        var remainingPayment = amount;

        foreach (var fee in feesToPay)
        {
            var remainingFee = GetTranAmt(fee);

            Transaction t = new()
            {
                AddDateTime = dateTime.HasValue ? dateTime.Value : DateTime.Now,
                AddToi = System.Security.Claims.ClaimsPrincipal.Current.Identity.Name,
                Comment = comment,
                ContainerId = fee.ContainerId,
                CustomerId = fee.CustomerId,
                CustomerType = fee.CustomerType,
                ObjectCode = fee.ObjectCode,
                Sequence = sequence++,
                ServiceAddressId = fee.ServiceAddressId
            };

            var transactionAmount = remainingFee < remainingPayment ? remainingFee : remainingPayment;
            if (transactionAmount > 0)
            {
                code.Process(transactionAmount, lastTransaction, t);
                db.Transactions.Add(t); //AddTransaction(t)
                lastTransaction = t;

                fee.ChgDateTime = dateTime.HasValue ? dateTime.Value : DateTime.Now;
                fee.ChgToi = System.Security.Claims.ClaimsPrincipal.Current.Identity.Name;
                fee.Partial = fee.Partial.GetValueOrDefault() + transactionAmount;
                if (code.IsCollections)
                    fee.PaidFull = fee.CollectionsAmount - fee.Partial.Value <= 0;
                else if (code.IsCounselors)
                    fee.PaidFull = fee.CounselorsAmount - fee.Partial.Value <= 0;
                else if (code.IsUncollectable)
                    fee.PaidFull = fee.UncollectableAmount - fee.Partial.Value <= 0;
                //UpdateTransaction(fee)

                remainingPayment -= transactionAmount;
            }

            if (remainingPayment <= 0)
                break;
        }
        if (remainingPayment > 0)
        {
            Transaction t = new()
            {
                AddDateTime = dateTime.HasValue ? dateTime.Value : DateTime.Now,
                AddToi = System.Security.Claims.ClaimsPrincipal.Current.Identity.Name,
                Comment = "overpay",
                CustomerId = lastTransaction.CustomerId,
                CustomerType = lastTransaction.CustomerType,
                Sequence = sequence++
            };
            code.Process(remainingPayment, lastTransaction, t);
            db.Transactions.Add(t); //AddTransaction(t)
        }
        return null;
    }

    private static async Task<ICollection<Transaction>> GetDelinquencyFeesToPay(SwDbContext db, int customerId, TransactionCode code)
    {
        IQueryable<Transaction> query = db.Transactions
            .Where(t => !t.DeleteFlag)
            .Where(t => t.CustomerId == customerId)
            .Where(t => t.PaidFull == null || t.PaidFull == false);

        if (code.IsCollections)
        {
            query = query.Where(t => t.CollectionsAmount > 0);
        }
        else if (code.IsCounselors)
        {
            query = query.Where(t => t.CounselorsAmount > 0);
        }
        else if (code.IsUncollectable)
        {
            query = query.Where(t => t.UncollectableAmount > 0);
        }
        else
        {
            return Array.Empty<Transaction>();
        }

        return await query
            .OrderBy(t => t.AddDateTime)
            .ThenBy(t => t.Sequence)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }

    private static async Task<Transaction> GetLatestTransaction(SwDbContext db, int customerId)
    {
        return await db.Transactions
            .Where(t => t.CustomerId == customerId && !t.DeleteFlag)
            .Include(t => t.TransactionCode)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ThenByDescending(t => t.Id)
            .FirstOrDefaultAsync();
    }

#endregion

    public async Task<TransactionHolding> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionHoldings
            .Where(c => c.TransactionHoldingId == id)
            .Include(c => c.TransactionCode)
            .SingleOrDefaultAsync();
    }

    public async Task<ICollection<TransactionHolding>> GetAllAwaitingPaymentByCustomerId(int customerId, bool includeDeleted)
    {
        using var db = dbFactory.CreateDbContext();
        var customer = await db.GetCustomerById(customerId);

        IQueryable<TransactionHolding> query = db.TransactionHoldings
            .Where(e => e.CustomerId == customer.CustomerId && e.Status == "Awaiting Payment")
            .Include(e => e.TransactionCode);

        if (!includeDeleted)
            query = query.Where(e => !e.DeleteFlag);

        return await query
            .OrderByDescending(e => e.AddDateTime)
            .AsNoTracking()
            .ToListAsync();
    }
}
