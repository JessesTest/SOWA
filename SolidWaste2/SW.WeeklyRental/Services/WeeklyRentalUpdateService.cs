using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.WeeklyRental.Services;

/// <summary>
/// Runs everyday (6:50 AM)
/// </summary>
public class WeeklyRentalUpdateService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    private SwDbContext db;
    private WeeklyRentalContext context;
    private List<TransactionCode> billing_transaction_code_records;

    public WeeklyRentalUpdateService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task Handle(WeeklyRentalContext context)
    {
        using var dbContext = dbFactory.CreateDbContext();
        db = dbContext;
        this.context = context;

        billing_transaction_code_records = await db.GetTransactionCodes();

        await Process();

        //Console.WriteLine("B4 Email and Transaction Add: " + cust_recs_read + " of " + TransactionsAdded + " | End 1 ")
        Console.WriteLine($"B4 Email and Transaction Add: added {context.TransactionsCreated} transactions | End 1");

        await dbContext.SaveChangesAsync();
        db = null;
        this.context = null;
    }

    public async Task Process()
    {
        //var customer_records = await db.GetCustomers();
        ////var cust_recs_read = 0
        //foreach (var customer in customer_records)
        //{
        //    //cust_recs_read += 1
        //    await ProcessCustomer(customer);
        //}

        var containers = await db.GetContainers(context.CurrentDate);
        foreach (var container in containers)
        {
            await ProcessContainer(container);
        }
    }

    //public async Task ProcessCustomer(Customer customer)
    //{
    //    if (customer.ContractCharge != null)
    //        return;

    //    if (customer.CancelDate != null)
    //        return;

    //    var service_address_records = await db.GetServiceAddressesByCustomer(customer.CustomerId);
    //    foreach(var sa in service_address_records)
    //    {
    //        await ProcessServiceAddress(sa);
    //    }
    //}

    //private async Task ProcessServiceAddress(ServiceAddress service_address)
    //{
    //    if (service_address.CancelDate != null)
    //        return;

    //    var containers = service_address.Containers;
    //    foreach(var container in containers)
    //    {
    //        await ProcessContainer(container);
    //    }
    //}

    private async Task ProcessContainer(Container container)
    {
        if (container.EffectiveDate >= context.CurrentDate)
            return;

        if (container.CancelDate != null && container.CancelDate < context.CurrentTime)
            return;

        var container_code = container.ContainerCode;
        int grace_days;

        if (container_code.Type == "C" && container.ContainerSubtypeId == 2)
        {
            // Commercial Temporary
            grace_days = 8;
        }
        else if (container_code.Type == "O")
        {
            // RollOff Temporary
            grace_days = 4;
        }
        else if (container_code.Type == "M" && container.ContainerSubtypeId == 7)
        {
            // Recycling Temporary Cardboard
            grace_days = 4;
        }
        else if(container_code.Type == "M" && container.ContainerSubtypeId == 8)
        {
            // Recycling Temporary Paper
            grace_days = 4;

        }
        else if(container_code.Type == "M" && container.ContainerSubtypeId == 9)
        {
            // Recycling Temporary Single Stream)
            grace_days = 4;
        }
        else
        {
            return;
        }

        var calc_eff_date = container.EffectiveDate;
        var rental_due = Calculate_Weekly_Rental_Due(grace_days, calc_eff_date, context.CurrentTime);
        if (!rental_due)
            return;

        var containerRate = await db.GetContainerRate(
            container.ContainerCodeId,
            container.ContainerSubtypeId,
            container.NumDaysService,
            container.BillingSize,
            context.CurrentDate);

        if (containerRate == null)
            throw new InvalidOperationException(
                "Could not find a container rate for Type: " + container_code.Type +
                ", ID: " + container.ContainerCodeId +
                ", # Days Svc: " + container.NumDaysService +
                ", Billing Size: " + container.BillingSize +
                " and Effective Date: " + context.CurrentTime + ".");

        var customerId = container.ServiceAddress.CustomerId;
        var objectCode = containerRate.ObjectCode;
        var rental_amt = containerRate.RateAmount;

        await Add_Weekly_Rental_Transaction_Record(rental_amt, container, objectCode);
        Console.WriteLine($"Cu:{customerId} SA:{container.ServiceAddressId} Co:{container.Id} CT:{container.ContainerCodeId} CS:{container.ContainerSubtypeId} CR:{containerRate.ContainerRateId} {rental_amt:$0.00}");
    }
    private bool Calculate_Weekly_Rental_Due(int grace_days, DateTime calc_eff_date, DateTime current_date)
    {
        TimeSpan diff = current_date - calc_eff_date;
        var calculate_days = diff.Days;
        calculate_days = (calculate_days - grace_days);
        var remainder = (calculate_days % 7);
        if (calculate_days == 0)
        {
            context.GoodWriter.WriteLine("  New Weekly Rental Added calc= " + calculate_days + "   grace= " + grace_days + "   rem= " + remainder + "   diff= " + diff.Days);
            return true;
        }

        if (calculate_days < 0)
        {
            return false;
        }
        else if (remainder == 0)
        {
            context.GoodWriter.WriteLine("  Old Weekly Rental Added calc= " + calculate_days + "   grace= " + grace_days + "   rem= " + remainder + "   diff= " + diff.Days);
            return true;
        }
        else
        {
            return false;
        }
    }

    private async Task Add_Weekly_Rental_Transaction_Record(
        Decimal rental_amt,
        Container container,
        int objectCode  // aka save_object
        ) 
    {
        var mthly_bill_beg_datetime = context.BeginTime;
        var customerId = container.ServiceAddress.CustomerId;
        var customerType = container.ServiceAddress.CustomerType;

        var lastTransaction = await db.GetLastTransaction(customerId);
        var collectionsBalance = lastTransaction?.CollectionsBalance ?? 0;
        var counselorsBalance = lastTransaction?.CounselorsBalance ?? 0;
        var transactionBalance = (lastTransaction?.TransactionBalance ?? 0) + rental_amt;
        var uncollectableBalance = lastTransaction?.UncollectableBalance ?? 0;
        var paidFull = transactionBalance <= 0;

        var comment = $"{mthly_bill_beg_datetime:MMM dd yyyy} WEEKLY RENTAL".ToUpper();

        TransactionCode transactionCode = container.ContainerCode.Type switch
        {
            "C" => billing_transaction_code_records[0],
            "O" => billing_transaction_code_records[1],
            _ => billing_transaction_code_records[2]
        };

        Transaction transaction = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = "Weekly Rental",
            //AssociatedTransaction = null,
            //AssociatedTransactionId = null,
            //BillMasters = null,
            //CheckNumber = null,
            //ChgDateTime = null,
            //ChgToi = null,
            CollectionsAmount = 0,
            CollectionsBalance = collectionsBalance,
            Comment = comment,
            //Container = null,
            ContainerId = container.Id,
            CounselorsAmount = 0,
            CounselorsBalance = counselorsBalance,
            //Customer = null,
            CustomerId = customerId,
            CustomerType = customerType,
            //DelDateTime = null,
            //DeleteFlag = false,
            //DelToi = null,
            //Id = 0,
            //InverseAssociatedTransaction = null,
            ObjectCode = objectCode,
            PaidFull = paidFull,
            //Partial = null,
            //PastDues = null,
            Sequence = context.TransactionsCreated++,
            //ServiceAddress = null,
            ServiceAddressId = container.ServiceAddressId,
            TransactionAmt = rental_amt,
            TransactionBalance = transactionBalance,
            //TransactionCode = null,
            TransactionCodeId = transactionCode.TransactionCodeId,
            //TransactionCodeRules = null,
            //TransactionHolding = null,
            //TransactionHoldingId = null,
            UncollectableAmount = 0,
            UncollectableBalance = uncollectableBalance
            //WorkOrder = null
        };

        db.Transactions.Add(transaction);

        context.GoodWriter.WriteLine(customerId + "  " + container.Id + "  " + container.ContainerCode.Type + " Weekly Rental Added ");
    }

}
