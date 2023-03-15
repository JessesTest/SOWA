using Microsoft.EntityFrameworkCore;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<SwDbContext> contextFactory;

    public TransactionService(IDbContextFactory<SwDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    internal async Task<bool> TransactionExists(int transactionId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Transactions.AnyAsync(e => e.Id == transactionId);
    }

    public async Task AddTransaction(Transaction transaction)
    {
        using var db = contextFactory.CreateDbContext();
        var lastTransaction = await db.Transactions
            .Where(e => e.CustomerId == transaction.CustomerId && !e.DeleteFlag)
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .FirstOrDefaultAsync();

        if (lastTransaction != null && transaction.AddDateTime.ToString("G") == lastTransaction.AddDateTime.ToString("G"))
        {
            transaction.Sequence = lastTransaction.Sequence + 1;
        }

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Transaction>> GetByCustomer(int customerId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Transactions
            .Where(e => e.CustomerId == customerId && !e.DeleteFlag)
            .Include(e => e.Customer)
            .Include(e => e.TransactionCode)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transaction> GetLatest(int customerId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Transactions
            .Where(e => e.CustomerId == customerId && !e.DeleteFlag)
            .OrderByDescending(e => e.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ThenByDescending(t => t.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetCurrentBalance(int customerId)
    {
        return (await GetLatest(customerId))?.TransactionBalance ?? 0.00m;
    }

    public async Task<Transaction> GetById(int transactionId)
    {
        using var db = contextFactory.CreateDbContext();
        return await db.Transactions
            .Where(e => e.Id == transactionId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task AddKanpayTransaction(Transaction transaction, TransactionKanPayFee fee, int kanpayid, string user)
    {
        using var db = contextFactory.CreateDbContext();

        //START the following code was copied from #region Transaction (public void AddTransactionAutoCalc(Transaction transaction, string user)) to faciliate a DATA BASE transaction scope encompassing three tables.                                   
        transaction.AddToi = user;

        // Make sure there is an add toi
        if (string.IsNullOrWhiteSpace(transaction.AddToi))
            throw new ArgumentException("No user defined", nameof(user));

        // Make sure there is a valid customer id
        Customer customer = await db.GetCustomerById(transaction.CustomerId)
            ?? throw new ArgumentException("Invalid customer id");

        // Make sure there is a valid transaction code
        TransactionCode transactionCode = await db.GetTransactionCode(transaction.TransactionCodeId.GetValueOrDefault())
            ?? throw new ArgumentException("Invalid transaction code");

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
        Transaction lastTransaction = await db.Transactions
            .Where(e => e.CustomerId == transaction.CustomerId)
            .OrderByDescending(e => e.AddDateTime)
            .ThenByDescending(e => e.Sequence)
            .FirstOrDefaultAsync();

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
            transaction.UncollectableBalance = lastTransaction.UncollectableBalance;
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

        //SCMB-242
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
        var paymentPlan = await db.GetActivePaymentPlanByCustomer(customer.CustomerId, true);

        if (transactionCode.Code == "PP" && paymentPlan != null)
        {
            if (transaction.TransactionBalance <= 0)
            {
                paymentPlan.Canceled = true;
                //UpdatePaymentPlan(paymentPlan);

                customer.PaymentPlan = false;
                //UpdateCustomer(customer);
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
                    //UpdatePaymentPlanDetail(firstUnpaidPaymentPlanDetail);
                }

                if (remainingBalance != 0)
                {
                    var unpaidPaymentPlanDetails = paymentPlan.Details.Where(m => !m.Paid.GetValueOrDefault());
                    var count = unpaidPaymentPlanDetails.Count();

                    foreach (var item in unpaidPaymentPlanDetails)
                    {
                        item.PaymentTotal -= remainingBalance / count;
                        item.Amount -= remainingBalance / count;
                        //UpdatePaymentPlanDetail(item);
                    }
                }
            }

            if (!paymentPlan.Details.Any(m => !m.Paid.GetValueOrDefault()))
            {
                paymentPlan.Canceled = true;
                //UpdatePaymentPlan(paymentPlan);

                customer.PaymentPlan = false;
                //UpdateCustomer(customer);
            }
        }

        transaction.AddDateTime = DateTime.Now;

        //_transactionRepository.Add(transaction);
        //                                               END of code copied from AddTransactionAutoCalc to faciliate a transaction

        //post KanPay transaction                                        
        db.Transactions.Add(transaction);                //table #1    add transaction

        //post KanPay transaction fee
        fee.TransactionKanPayFeeAddDateTime = DateTime.Now;
        fee.TransactionKanPayFeeAddToi = user;
        db.TransactionKanPayFees.Add(fee);               //table #2    add fee

        //remove KanPay token                
        //var token = db.KanPay.Single(x => x.KanPayID == id);
        var token = db.KanPays.Find(kanpayid);           //reterive token with KanPayID
        db.KanPays.Remove(token);                        //table #3    remove token     

        db.SaveChanges();                               //savechanges to all three tables as DATABASE TRANSACTION

        //post transaction id to transactionkanpayfee table
        fee.TransactionId = transaction.Id.ToString();
        //_transactionKanPayFeeRepository.Update(fee);    //update fee.TransactionId with transactionid from db.savechanges from the transaction table add

        await db.SaveChangesAsync();
    }
}
