using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using PE.DM;
using SW.BLL.DTOs;
using SW.BLL.Extensions;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;
    private readonly IPersonEntityService _personEntityService;

    public TransactionService(IDbContextFactory<SwDbContext> dbFactory, IPersonEntityService personEntityService)
    {
        this.dbFactory = dbFactory;
        _personEntityService = personEntityService;
    }

    #region Transaction

    internal async Task<bool> TransactionExists(int transactionId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Transactions.AnyAsync(e => e.Id == transactionId);
    }

    public async Task AddTransaction(Transaction transaction)
    {
        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(transaction.CustomerId);

        var lastTransaction = await db.Transactions
            .Where(e => e.CustomerId == customer.CustomerId && !e.DeleteFlag)
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

    public async Task<string> AddTransactionAutoCalc(Transaction transaction)
    {
        using var db = dbFactory.CreateDbContext();

        // Make sure there is an add toi
        if (string.IsNullOrWhiteSpace(transaction.AddToi))
            return "No user defined";

        // Make sure there is a valid customer id
        var customer = await db.GetCustomerById(transaction.CustomerId);
        if (customer == null)
            return "Invalid customer id";

        // Make sure there is a valid transaction code
        var transactionCode = await db.GetTransactionCode(transaction.TransactionCodeId.GetValueOrDefault());
        if (transactionCode == null)
            return "Invalid transaction code";

        // If an associated transaction is provided, make sure it is valid
        if (transaction.AssociatedTransactionId.HasValue && !await TransactionExists(transaction.AssociatedTransactionId.Value))
            return "Invalid associated transaction id";

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
        var lastTransaction = await GetLatest(transaction.CustomerId);

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
                transactionAmount -= counselorsAmount;
            }

            transaction.TransactionAmt = -transactionAmount;

            if (counselorsAmount > 0)
            {
                var result = await MakeDelinquencyPayment(customer.CustomerId, "PCC", counselorsAmount, transaction.Comment);
                if (result != null)
                    return result;
            }

            if (transaction.TransactionAmt == 0)
                return null;

            lastTransaction = await GetLatest(transaction.CustomerId);
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

        transaction.UncollectableAmount ??= 0;

        // Add the new transaction amount to the balance
        transaction.TransactionBalance += transaction.TransactionAmt;
        transaction.CounselorsBalance += transaction.CounselorsAmount;
        transaction.CollectionsBalance += transaction.CollectionsAmount;
        transaction.UncollectableBalance += transaction.UncollectableAmount;

        // "Pay off" payment plans if customer is on one
        var paymentPlan = (await db.GetPaymentPlanByCustomer(customer.CustomerId, true)).SingleOrDefault(m => m.Status == "Active");

        if (transactionCode.Code == "PP" && paymentPlan != null)
        {
            if (transaction.TransactionBalance <= 0)
            {
                paymentPlan.Canceled = true;
                db.PaymentPlans.Update(paymentPlan);

                customer.PaymentPlan = false;
                db.Customers.Update(customer);
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
                    db.PaymentPlanDetails.Update(firstUnpaidPaymentPlanDetail);
                }

                if (remainingBalance != 0)
                {
                    var unpaidPaymentPlanDetails = paymentPlan.Details.Where(m => !m.Paid.GetValueOrDefault());
                    var count = unpaidPaymentPlanDetails.Count();

                    foreach (var item in unpaidPaymentPlanDetails)
                    {
                        item.PaymentTotal -= remainingBalance / count;
                        item.Amount -= remainingBalance / count;
                        db.PaymentPlanDetails.Update(item);
                    }
                }
            }

            if (!paymentPlan.Details.Any(m => !m.Paid.GetValueOrDefault()))
            {
                paymentPlan.Canceled = true;
                db.PaymentPlans.Update(paymentPlan);

                customer.PaymentPlan = false;
                db.Customers.Update(customer);
            }
        }

        transaction.AddDateTime = DateTime.Now;

        if (lastTransaction != null && transaction.AddDateTime.ToString("G") == lastTransaction.AddDateTime.ToString("G"))
            transaction.Sequence = lastTransaction.Sequence + 1;

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<ICollection<Transaction>> GetByCustomer(int customerId, bool includeDeleted)
    {
        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);

        IQueryable<Transaction> query = db.Transactions
            .Where(e => e.CustomerId == customer.CustomerId)
            .Include(e => e.Customer)
            .Include(e => e.TransactionCode);

        if (!includeDeleted)
            query = query.Where(e => !e.DeleteFlag);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> GetCurrentBalance(int customerId)
    {
        return (await GetLatest(customerId))?.TransactionBalance ?? 0.00m;
    }

    public async Task<Transaction> GetLatest(int customerId)
    {
        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);

        return await db.Transactions
            .Where(e => e.CustomerId == customer.CustomerId && !e.DeleteFlag)
            .OrderByDescending(e => e.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .ThenByDescending(t => t.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Transaction> GetById(int transactionId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.Transactions
            .Where(e => e.Id == transactionId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<Transaction>> GetAllUnpaidLateFeesByCustomerId(int customerId)
    {
        using var db = dbFactory.CreateDbContext();
        var customer = await db.GetCustomerById(customerId);
        var code = await db.GetTransactionCodeByCode("LF");

        return await db.Transactions
            .Where(e => e.CustomerId == customer.CustomerId
                && e.TransactionCodeId == code.TransactionCodeId
                && (!e.PaidFull.HasValue || !e.PaidFull.Value)
                && !e.DeleteFlag)
            .Include(e => e.TransactionCode)
            .AsNoTracking()
            .ToListAsync();
    }

    #endregion

    #region Delinquency

    public async Task<decimal> GetRemainingBalanceFromLastBill(int customerId)
    {
        return await GetRemainingCurrentBalance(0, customerId);
    }

    internal async Task<decimal> GetPastDueAmount(int daysPastDue, int customerId)
    {
        return await GetRemainingCurrentBalance(daysPastDue + 14, customerId);
    }

    public async Task<decimal> GetPastDueAmount(int customerId)
    {
        return await GetPastDueAmount(1, customerId);
    }

    public async Task<decimal> Get30DaysPastDueAmount(int customerId)
    {

        return await GetPastDueAmount(30, customerId);
    }

    public async Task<decimal> Get60DaysPastDueAmount(int customerId)
    {

        return await GetPastDueAmount(60, customerId);
    }

    public async Task<decimal> Get90DaysPastDueAmount(int customerId)
    {

        return await GetPastDueAmount(90, customerId);
    }

    internal async Task<decimal> GetRemainingCurrentBalance(int days, int customerId)
    {
        return await GetRemainingCurrentBalance(DateTime.Now, days, customerId);
    }

    public async Task<decimal> GetRemainingCurrentBalance(DateTime date, int days, int customerId)
    {
        using var db = dbFactory.CreateDbContext();

        var endDate = date.Date.AddDays(1);
        var startDate = date.Date.AddDays(-days);
        string[] billTypes = { "MB", "FB", "MBR" };
        var customer = await db.GetCustomerById(customerId);

        var bill = await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId
                && !t.DeleteFlag
                && billTypes.Contains(t.TransactionCode.Code)
                && t.AddDateTime < startDate)
            .OrderByDescending(t => t.AddDateTime).ThenByDescending(t => t.Sequence).ThenByDescending(t => t.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (bill == null || bill.TransactionBalance <= 0)
            return 0;

        var transactions = await db.Transactions
            .Where(t => t.CustomerId == customer.CustomerId
                && !t.DeleteFlag
                && t.AddDateTime < endDate
                && t.TransactionAmt < 0
                && ((t.AddDateTime > bill.AddDateTime) || (t.AddDateTime == bill.AddDateTime && t.Sequence > bill.Sequence)))
            .AsNoTracking()
            .ToListAsync();

        var total = bill.TransactionBalance + transactions.Sum(t => t.TransactionAmt);
        if (total < 0)
            return 0;

        return total;
    }

    public async Task<decimal> GetCollectionsBalance(int customerId)
    {
        var t = await GetLatest(customerId);
        return t != null ? t.CollectionsBalance : 0;
    }

    public async Task<decimal> GetCounselorsBalance(int customerId)
    {
        var t = await GetLatest(customerId);
        return t != null ? t.CounselorsBalance : 0;
    }

    public async Task<ICollection<CustomerDelinquency>> GetAllDelinquencies()
    {
        using var db = dbFactory.CreateDbContext();

        var list = new List<CustomerDelinquency>();
        var customers = await db.Customers
            .AsNoTracking()
            .ToListAsync();

        foreach (var c in customers)
        {
            var d = new CustomerDelinquency();
            d.Customer = c;
            d.CollectionsBalance = await GetCollectionsBalance(c.CustomerId);
            d.CounselorsBalance = await GetCounselorsBalance(c.CustomerId);

            var d1 = await GetPastDueAmount(c.CustomerId);

            if (d1 > 0)
            {
                var d30 = await Get30DaysPastDueAmount(c.CustomerId);
                var d60 = await Get60DaysPastDueAmount(c.CustomerId);
                var d90 = await Get90DaysPastDueAmount(c.CustomerId);

                d.PastDue = d1 - d30;
                if (d.PastDue < 0)
                    d.PastDue = 0;

                d.PastDue30Days = d30 - d60;
                if (d.PastDue30Days < 0)
                    d.PastDue30Days = 0;

                d.PastDue60Days = d60 - d90;
                if (d.PastDue60Days < 0)
                    d.PastDue60Days = 0;

                d.PastDue90Days = d90;
                if (d.PastDue90Days < 0)
                    d.PastDue90Days = 0;
            }

            if (d.IsDelinquent)
            {
                d.PersonEntity = await _personEntityService.GetById(c.Pe);
                list.Add(d);
            }
        }

        return list;
    }

    public async Task<ICollection<Transaction>> GetPayments(DateTime thruDate, Transaction bill)
    {
        using var db = dbFactory.CreateDbContext();

        var date = thruDate.Date.AddDays(1);
        return await db.Transactions
            .Where(t => !t.DeleteFlag
                && t.TransactionAmt < 0
                && t.CustomerId == bill.CustomerId
                && t.AddDateTime < date
                && ((t.AddDateTime > bill.AddDateTime) || (t.AddDateTime == bill.AddDateTime && t.Sequence > bill.Sequence)))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<Transaction>> GetLatestTransactionsWithDelinquency()
    {
        using var db = dbFactory.CreateDbContext();

        var groupTran = await db.Transactions
            .Where(t => !t.DeleteFlag)
            .GroupBy(t => t.CustomerId)
            .Select(t => t.Max(gt => gt.Id))
            .ToListAsync();

        var temp = await (
            from t in db.Transactions
            where groupTran.Contains(t.Id) &&
            (t.CollectionsBalance > 0 || t.CounselorsBalance > 0 || t.UncollectableBalance > 0)
            select t)
            .ToListAsync();

        var bob = new List<Transaction>();
        var customerIds = temp.Select(t => t.CustomerId).Distinct().ToList();
        foreach (var customerId in customerIds)
        {
            bob.Add(temp.Where(t => t.CustomerId == customerId).OrderBy(t => t.Id).Last());
        }

        foreach (var tom in bob)
        {
            tom.Customer = await db.Customers.Where(c => c.CustomerId == tom.CustomerId).SingleOrDefaultAsync();
            tom.Customer.PersonEntity = await _personEntityService.GetById(tom.Customer.Pe);
            if (tom.Customer.PersonEntity == null)
                tom.Customer.PersonEntity = new PersonEntity();
        }

        return bob;
    }

    public async Task<string> MakeDelinquencyPayment(int customerId, string transactionTypeCode, decimal amount, string comment)
    {
        var userName = System.Security.Claims.ClaimsPrincipal.Current.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();

        var customer = await db.GetCustomerById(customerId);
        if (customer == null || customer.DelDateTime.HasValue)
            return string.Format("Customer {0} not found", customerId);

        var code = await db.GetTransactionCodeByCode(transactionTypeCode);
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

        var feesToPay = await db.GetDelinquencyFeesToPay(customerId, code);
        var lastTransaction = await GetLatest(customerId);
        var sequence = 0;
        var remainingPayment = amount;

        foreach (var fee in feesToPay)
        {
            var remainingFee = GetTranAmt(fee);

            var t = new Transaction
            {
                AddDateTime = DateTime.Now,
                AddToi = userName,
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
                await db.AddTransaction(t);
                lastTransaction = t;

                fee.ChgDateTime = DateTime.Now;
                fee.ChgToi = userName;
                fee.Partial = fee.Partial.GetValueOrDefault() + transactionAmount;
                if (code.IsCollections)
                    fee.PaidFull = fee.CollectionsAmount - fee.Partial.Value <= 0;
                else if (code.IsCounselors)
                    fee.PaidFull = fee.CounselorsAmount - fee.Partial.Value <= 0;
                else if (code.IsUncollectable)
                    fee.PaidFull = fee.UncollectableAmount - fee.Partial.Value <= 0;
                db.Transactions.Update(fee);

                remainingPayment -= transactionAmount;
            }

            if (remainingPayment <= 0)
                break;
        }
        if (remainingPayment > 0)
        {
            var t = new Transaction
            {
                AddDateTime = DateTime.Now,
                AddToi = userName,
                Comment = "overpay",
                CustomerId = lastTransaction.CustomerId,
                CustomerType = lastTransaction.CustomerType,
                Sequence = sequence++
            };
            code.Process(remainingPayment, lastTransaction, t);
            await db.AddTransaction(t);
        }

        await db.SaveChangesAsync();
        return null;
    }

    #endregion

    #region KanPay

    public async Task AddKanpayTransaction(Transaction transaction, TransactionKanPayFee fee, int kanpayid, string user)
    {
        using var db = dbFactory.CreateDbContext();

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
            .Where(e => e.CustomerId == transaction.CustomerId && !e.DeleteFlag)
            .OrderByDescending(e => e.Sequence)
            .ThenByDescending(e => e.AddDateTime)
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

    #endregion

    public async Task<ICollection<TransactionListingItem>> GetListingByCustomer(int customerId, bool includeDeleted = false)
    {
        using var db = dbFactory.CreateDbContext();
        IQueryable<Transaction> query = db.Transactions
            .Where(e => e.CustomerId == customerId);

        if (!includeDeleted)
            query = query.Where(e => !e.DeleteFlag);

        return await query
            .Select(e => new TransactionListingItem
             {
                 AddDateTime = e.AddDateTime,
                 CodeDescription = e.TransactionCode.Description,
                 TransactionComment = e.Comment,
                 TransactionAmount = e.TransactionAmt,
                 TransactionBalance = e.TransactionBalance,
                 TransactionCode = e.TransactionCode.Code,
                 TransactionID = e.Id
             })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DateTime> GetLastBillTranDateTime(int customerID, DateTime billAddDateTime)
    {
        var codes = new[] { "MB", "MBR", "FB" };

        using var db = dbFactory.CreateDbContext();
        var temp = await db.Transactions
            .Where(t => t.CustomerId == customerID)
            .Where(t => !t.DeleteFlag)
            .Where(t => t.AddDateTime < billAddDateTime)
            .Where(t => codes.Contains(t.TransactionCode.Code))
            .OrderByDescending(t => t.AddDateTime)
            .ThenByDescending(t => t.Sequence)
            .FirstOrDefaultAsync();

        if (temp != null)
            return temp.AddDateTime;

        return new DateTime(billAddDateTime.Year, billAddDateTime.Month, 1).AddMonths(-1);
    }
}
