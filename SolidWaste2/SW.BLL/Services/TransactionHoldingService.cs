using Common.Extensions;
using Common.Services.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;
using System.ComponentModel.DataAnnotations;

namespace SW.BLL.Services;

public class TransactionHoldingService : ITransactionHoldingService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly int cutoffDays = 30;
    private readonly ITransactionService transactionService;

    public TransactionHoldingService(IDbContextFactory<SwDbContext> dbFactory, ITransactionService transactionService)
    {
        this.dbFactory = dbFactory;
        this.transactionService = transactionService;
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
            query.Where(th => string.IsNullOrEmpty(th.Security)) :
            query.Where(th => string.IsNullOrEmpty(th.Security) || th.Security == email);

        var approvedStatuses = new[] { "Resolved", "Approved", "Rejected" };
        var cutoffDate = DateTime.Today.AddDays(-cutoffDays);
        query = query
            .Where(th => th.ChgDateTime == null || th.ChgDateTime >= cutoffDate || (th.ChgDateTime < cutoffDate && !approvedStatuses.Contains(th.Status)));

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
            .Where(th => th.ChgDateTime == null || th.ChgDateTime >= cutoffDate || (th.ChgDateTime < cutoffDate && !approvedStatuses.Contains(th.Status)));

        return await query
            .Include(th => th.TransactionCode)
            .OrderByDescending(th => th.AddDateTime)
            .SingleOrDefaultAsync();
    }

    public async Task<bool> IsAuthorized(int transactionHoldingId, string email)
    {
        using var db = dbFactory.CreateDbContext();

        IQueryable<TransactionHolding> query = db.TransactionHoldings
            .Where(th => !th.DeleteFlag);

        query = string.IsNullOrWhiteSpace(email) ?
            query.Where(th => string.IsNullOrEmpty(th.Security)) :
            query.Where(th => string.IsNullOrEmpty(th.Security) || th.Security == email);

        var approvedStatuses = new[] { "Resolved", "Approved", "Rejected" };
        var cutoffDate = DateTime.Today.AddDays(-cutoffDays);
        query = query
            .Where(th => th.ChgDateTime == null || th.ChgDateTime >= cutoffDate || (th.ChgDateTime < cutoffDate && !approvedStatuses.Contains(th.Status)));

        return await query.AnyAsync(t => t.TransactionHoldingId == transactionHoldingId);
    }

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

    #region Add/Update/Delete

    public async Task Add(TransactionHolding transactionHolding)
    {
        _ = transactionHolding ?? throw new ArgumentNullException(nameof(transactionHolding));
        Validator.ValidateObject(transactionHolding, new ValidationContext(transactionHolding));

        using var db = dbFactory.CreateDbContext();

        await ValidateTransactionHolding(transactionHolding);

        transactionHolding.AddDateTime = DateTime.Now;

        db.TransactionHoldings.Add(transactionHolding);
        await db.SaveChangesAsync();
    }

    public async Task Update(TransactionHolding transactionHolding)
    {
        _ = transactionHolding ?? throw new ArgumentNullException(nameof(transactionHolding));
        Validator.ValidateObject(transactionHolding, new ValidationContext(transactionHolding));

        using var db = dbFactory.CreateDbContext();

        await ValidateTransactionHolding(transactionHolding);

        // Make sure there is a chg toi
        if (string.IsNullOrWhiteSpace(transactionHolding.ChgToi))
            throw new ArgumentException("No user defined");

        transactionHolding.ChgDateTime = DateTime.Now;

        db.TransactionHoldings.Update(transactionHolding);
        await db.SaveChangesAsync();
    }

    public async Task Delete(TransactionHolding transactionHolding)
    {
        _ = transactionHolding ?? throw new ArgumentNullException(nameof(transactionHolding));

        using var db = dbFactory.CreateDbContext();

        transactionHolding.DelDateTime = DateTime.Now;
        transactionHolding.DeleteFlag = true;

        db.TransactionHoldings.Update(transactionHolding);
        await db.SaveChangesAsync();
    }

    #endregion

    #region Resolve/Approve/Reject

    public async Task<string> Resolve(TransactionHolding transactionHolding, string email, string displayName)
    {
        using var db = dbFactory.CreateDbContext();
        string result;
        Transaction associatedTransaction = null;

        result = Resolve_ValidateTransactionHolding(transactionHolding, email);
        if (result != null)
            return result;

        // If an associated transaction is provided, make sure it is valid
        if (transactionHolding.AssociatedTransactionId.HasValue)
        {
            associatedTransaction = await db.GetTransactionById(transactionHolding.AssociatedTransactionId.Value);

            result = Resolve_ValidateAssociatedTransaction(transactionHolding, associatedTransaction);
            if (result != null)
                return result;
        }

        // Make sure user has security to resolve this item
        if (!string.IsNullOrWhiteSpace(transactionHolding.Security))
        {
            var authorized = await IsAuthorized(transactionHolding.TransactionHoldingId, email);
            if (!authorized)
                return "User not authorized to resolve this TransactionHolding";
        }

        var customer = await db.GetCustomerById(transactionHolding.CustomerId);

        if (transactionHolding.BatchId > 0)
        {
            transactionHolding.Sender = string.Concat(transactionHolding.Sender, " ", transactionHolding.BatchId);
        }

        var transaction = new Transaction
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
            AddToi = transactionHolding.Sender
        };

        result = await transactionService.AddTransactionAutoCalc(transaction);
        if (result != null)
            return result;

        if (associatedTransaction != null)
        {
            associatedTransaction.Partial = associatedTransaction.TransactionAmt;
            associatedTransaction.PaidFull = true;
            associatedTransaction.ChgToi = displayName;
            associatedTransaction.ChgDateTime = DateTime.Now;
            db.Transactions.Update(associatedTransaction);
        }

        transactionHolding.Status = "Resolved";
        transactionHolding.Approver = email;
        transactionHolding.ChgToi = displayName;
        transactionHolding.ChgDateTime = DateTime.Now;
        db.TransactionHoldings.Update(transactionHolding);

        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string> Approve(TransactionHolding transactionHolding, string email, string displayName)
    {
        using var db = dbFactory.CreateDbContext();
        string result;
        Transaction associatedTransaction = null;

        result = Resolve_ValidateTransactionHolding(transactionHolding, email);
        if (result != null)
            return result;

        // If an associated transaction is provided, make sure it is valid
        if (transactionHolding.AssociatedTransactionId.HasValue)
        {
            associatedTransaction = await db.GetTransactionById(transactionHolding.AssociatedTransactionId.Value);

            result = Resolve_ValidateAssociatedTransaction(transactionHolding, associatedTransaction);
            if (result != null)
                return result;
        }

        // Make sure user has security to resolve this item
        if (!string.IsNullOrWhiteSpace(transactionHolding.Security))
        {
            var authorized = await IsAuthorized(transactionHolding.TransactionHoldingId, email);
            if (!authorized)
                return "User not authorized to approve this TransactionHolding";
        }

        var customer = await db.GetCustomerById(transactionHolding.CustomerId);

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
            AddToi = displayName
        };

        result = await transactionService.AddTransactionAutoCalc(transaction);
        if (result != null)
            return result;

        if (associatedTransaction != null)
        {
            associatedTransaction.Partial = associatedTransaction.TransactionAmt;
            associatedTransaction.PaidFull = true;
            associatedTransaction.ChgToi = displayName;
            associatedTransaction.ChgDateTime = DateTime.Now;
            db.Transactions.Update(associatedTransaction);
        }

        transactionHolding.Status = "Approved";
        transactionHolding.Approver = email;
        transactionHolding.ChgToi = displayName;
        transactionHolding.ChgDateTime = DateTime.Now;
        db.TransactionHoldings.Update(transactionHolding);

        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string> Reject(TransactionHolding transactionHolding, string email, string displayName)
    {
        using var db = dbFactory.CreateDbContext();

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

    #endregion

    #region Batch logic

    public async Task<bool> IsBatchIdValid(int batchId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionHoldings.AnyAsync(t => t.BatchId == batchId && t.Status == "Awaiting Submit" && !t.DeleteFlag);
    }

    public async Task<ICollection<TransactionHolding>> GetBatchTransactionsById(int batchId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.TransactionHoldings
            .Where(t => t.BatchId == batchId
                && t.Status == "Awaiting Submit"
                && !t.DeleteFlag)
            .OrderByDescending(t => t.TransactionHoldingId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetNextBatchId()
    {
        using var db = dbFactory.CreateDbContext();
        var currentBatchId = await db.TransactionHoldings
            .OrderByDescending(t => t.BatchId)
            .Select(t => t.BatchId)
            .FirstOrDefaultAsync();

        return currentBatchId + 1;
    }

    public async Task<IDictionary<int, string>> GetActiveBatches()
    {
        using var db = dbFactory.CreateDbContext();

        var activeHoldings = await db.TransactionHoldings
            .Where(t => t.BatchId != 0
                && t.Status == "Awaiting Submit"
                && !t.DeleteFlag)
            .AsNoTracking()
            .ToListAsync();

        return activeHoldings
            .GroupBy(t => t.BatchId)
            .ToDictionary(g => g.Key, g => g.First().AddToi);
    }

    #endregion

    #region Utility

    internal async Task ValidateTransactionHolding(TransactionHolding transactionHolding)
    {
        using var db = dbFactory.CreateDbContext();

        // Make sure there is an add toi
        if (string.IsNullOrWhiteSpace(transactionHolding.AddToi))
            throw new ArgumentException("No user defined");

        // Make sure there is a valid customer id
        var customer = await db.GetCustomerById(transactionHolding.CustomerId);
        if (customer == null)
            throw new ArgumentException("Invalid customer id");
        transactionHolding.CustomerId = customer.CustomerId;

        // Make sure there is a valid transaction code
        var transactionCode = await db.GetTransactionCode(transactionHolding.TransactionCodeId.GetValueOrDefault());
        if (transactionCode == null)
            throw new ArgumentException("Invalid transaction code");

        // Make sure the transaction amount has the right sign
        if (transactionCode.TransactionSign == "P" && transactionHolding.TransactionAmt < 0)
            transactionHolding.TransactionAmt *= -1;
        if (transactionCode.TransactionSign == "N" && transactionHolding.TransactionAmt > 0)
            transactionHolding.TransactionAmt *= -1;

        // If an associated transaction is provided, make sure it is valid
        if (transactionHolding.AssociatedTransactionId.HasValue)
        {
            var associatedTransaction = await db.GetTransactionById(transactionHolding.AssociatedTransactionId.Value);

            // Make sure transaction exists
            if (associatedTransaction == null)
                throw new ArgumentException("Invalid associated transaction id");

            // Make sure associated transaction is not already paid off
            if (associatedTransaction.PaidFull.HasValue && associatedTransaction.PaidFull.Value)
                throw new ArgumentException("Associated transaction already paid in full");

            // Make sure associated transaction is of a valid transaction type
            string[] associatedTransactionTypes = { "LF" };
            if (!associatedTransactionTypes.Contains(associatedTransaction.TransactionCode.Code))
                throw new ArgumentException("Associated transaction not a valid transaction type");

            // Make sure associated transaction value matches transaction value
            if (associatedTransaction.TransactionAmt + transactionHolding.TransactionAmt != 0)
                throw new ArgumentException("TransactionHolding amount does not match associated transaction amount");
        }
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
        if (false && transactionHolding.Sender.ToUpper().Trim() == email.ToUpper().Trim())
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

    #endregion
}
