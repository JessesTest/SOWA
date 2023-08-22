using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using PE.DAL.Contexts;
using SW.DM;
using PE.DM;
using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.Graph;

namespace SW.Billing.Services;

/// <summary>
/// Runs every 1st of the month (6:30 AM)
/// </summary>
public class BillingUpdateService
{
    private readonly IDbContextFactory<SwDbContext> swdbFactory;
    private readonly IDbContextFactory<PeDbContext> pedbFactory;

    private SwDbContext swdb;
    private PeDbContext pedb;

    private BillingContext context;

    int cust_recs_read = 0;

    bool process_customer = false;
    bool process_location = false;
    bool process_container = false;

    decimal balance_forward = 0;
    decimal past_due_amt = 0;

    bool final_billing = false;

    bool contract_charge_customer = false;

    bool credit_bal_final_billed_cancelled_acct = false;
    
    bool pos_bal_final_billed_cancelled_acct = false;

    bool past_due_bal_active_acct = false;
    bool past_due_bal_final_billed_cancelled_acct = false;
    bool past_due_bal_non_final_billed_cancelled_acct = false;

    int bill_service_address_records_inserted = 0;

    decimal customer_total, container_total, container_charge;

    public BillingUpdateService(IDbContextFactory<SwDbContext> swdbFactory, IDbContextFactory<PeDbContext> pedbFactory)
    {
        this.swdbFactory = swdbFactory;
        this.pedbFactory = pedbFactory;
    }

    public async Task Handle(BillingContext context) 
    {
        using var swdbContext = swdbFactory.CreateDbContext();
        swdb = swdbContext;

        using var pedbContext = pedbFactory.CreateDbContext();
        pedb = pedbContext;

        this.context = context;

        List<Transaction> monthly_billing_transaction_records = await swdb.GetLatestMonthlyBillingTransactions(context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime);
        if (!monthly_billing_transaction_records.Any())
        {
            await Process(swdbContext);
        }
        else
        {
            throw new InvalidOperationException("SW billing records already exist for the monthly billing period of " + string.Concat(context.Mthly_Bill_Beg_DateTime.Year.ToString().PadLeft(4, '0'), "-", context.Mthly_Bill_Beg_DateTime.Month.ToString().PadLeft(2, '0'), "-", context.Mthly_Bill_Beg_DateTime.Day.ToString().PadLeft(2, '0')) + " thru " + string.Concat(context.Mthly_Bill_End_DateTime.Year.ToString().PadLeft(4, '0'), "-", context.Mthly_Bill_End_DateTime.Month.ToString().PadLeft(2, '0'), "-", context.Mthly_Bill_End_DateTime.Day.ToString().PadLeft(2, '0')) + ".");
        }
    }
    
    public async Task Process(SwDbContext swdbContext) 
    {
        List<TransactionCode> billing_transaction_code_records = await swdb.GetBillingTransactionCodes();

        List<Customer> customer_records = await swdb.GetAllCustomers();

        List<BillMaster> bill_master_records = new List<BillMaster>();

        foreach (var customer in customer_records) 
        {
            //if (customer.CustomerId != 106170)
            //{
            //    continue;
            //}

            cust_recs_read += 1;

            Console.WriteLine(String.Concat("Processing Customer Record #: ", cust_recs_read, " of ", customer_records.Count, " | Cust #: ", customer.CustomerId));

            Init_SW_Billing_Variables();

            List<Transaction> transaction_records = await swdb.GetTransactionsByCustomerId(customer.CustomerId);

            if (Convert.ToDateTime(Convert.ToString(customer.EffectiveDate.Year.ToString().PadLeft(4, '0') + "-" + customer.EffectiveDate.Month.ToString().PadLeft(2, '0') + "-" + customer.EffectiveDate.Day.ToString().PadLeft(2, '0'))) <= Convert.ToDateTime(Convert.ToString(context.Mthly_Bill_End_DateTime.Year.ToString().PadLeft(4, '0') + "-" + context.Mthly_Bill_End_DateTime.Month.ToString().PadLeft(2, '0') + "-" + context.Mthly_Bill_End_DateTime.Day.ToString().PadLeft(2, '0')))) 
            {
                if (customer.CancelDate != null) 
                {
                    await Eval_Cancelled_Customer(customer, transaction_records, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime);

                    if (!process_customer && past_due_amt == 0 && balance_forward > 0 && customer.CancelDate < Convert.ToDateTime("2015-08-01 12:00:00 AM"))
                    {
                        past_due_amt = balance_forward;

                        past_due_bal_final_billed_cancelled_acct = true;

                        process_customer = true;
                    }
                }
                else 
                {
                    await Eval_Active_Customer(customer, transaction_records, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime);
                }
                
                if (process_customer)
                {
                    bill_service_address_records_inserted = 0;

                    customer_total = 0;

                    contract_charge_customer = Eval_Contract_Charge_Customer(customer);
                    
                    List<ServiceAddress> service_address_records = await swdb.GetServiceAddressByCustomer(customer.CustomerId);

                    List<BillServiceAddress> bill_service_address_records = new List<BillServiceAddress>();

                    if (past_due_bal_final_billed_cancelled_acct)
                    {
                        if (service_address_records.Count > 0)
                        {
                            PE.DM.Address pe_service_address = await pedb.GetAddressById(service_address_records.OrderByDescending(s => s.CancelDate).ThenBy(s => s.LocationNumber).First().PeaddressId);

                            List<BillContainerDetail> bill_container_detail_records = new List<BillContainerDetail>();

                            Add_Bill_Service_Address_Record(pe_service_address, service_address_records.OrderByDescending(s => s.CancelDate).ThenBy(s => s.LocationNumber).First(), bill_service_address_records, bill_container_detail_records);

                            bill_service_address_records_inserted += 1;
                        }
                    }
                    else
                    {
                        foreach (var service_address in service_address_records)
                        {
                            process_location = false;

                            decimal service_address_total = 0;

                            Eval_Service_Address_Status(service_address, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime);

                            if (process_location)
                            {
                                List<Container> container_records = await swdb.GetContainersByServiceAddress(service_address.Id);

                                List<BillContainerDetail> bill_container_detail_records = new List<BillContainerDetail>();

                                foreach (var container in container_records)
                                {
                                    process_container = false;
                                    bool prorate_container = false;

                                    container_charge = 0;
                                    container_total = 0;

                                    if (container.CancelDate != null && (Convert.ToDateTime(Convert.ToString(container.CancelDate.GetValueOrDefault().Year.ToString().PadLeft(4, '0') + "-" + container.CancelDate.GetValueOrDefault().Month.ToString().PadLeft(2, '0') + "-" + container.CancelDate.GetValueOrDefault().Day.ToString().PadLeft(2, '0'))) >= Convert.ToDateTime(Convert.ToString(context.Mthly_Bill_Beg_DateTime.Year.ToString().PadLeft(4, '0') + "-" + context.Mthly_Bill_Beg_DateTime.Month.ToString().PadLeft(2, '0') + "-" + context.Mthly_Bill_Beg_DateTime.Day.ToString().PadLeft(2, '0'))) && Convert.ToDateTime(Convert.ToString(container.CancelDate.GetValueOrDefault().Year.ToString().PadLeft(4, '0') + "-" + container.CancelDate.GetValueOrDefault().Month.ToString().PadLeft(2, '0') + "-" + container.CancelDate.GetValueOrDefault().Day.ToString().PadLeft(2, '0'))) <= Convert.ToDateTime(Convert.ToString(context.Mthly_Bill_End_DateTime.Year.ToString().PadLeft(4, '0') + "-" + context.Mthly_Bill_End_DateTime.Month.ToString().PadLeft(2, '0') + "-" + context.Mthly_Bill_End_DateTime.Day.ToString().PadLeft(2, '0')))))
                                    {
                                        prorate_container = true;
                                    }

                                    if (container.Delivered == "Delivered" || container.Delivered == "Scheduled for Pick Up" || container.Delivered == "Returned" || prorate_container)
                                    {
                                        Eval_Container_Status(container, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime);

                                        if (process_container)
                                        {
                                            ContainerSubtype container_subtype = await swdb.GetContainerSubtypeById(container.ContainerSubtypeId);

                                            if (container_subtype.BillingFrequency != "Weekly")
                                            {
                                                ContainerCode container_code = await swdb.GetContainerCodeById(container.ContainerCodeId);

                                                container_total = await Calculate_Container_Charge(customer, container_code, container, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime, bill_container_detail_records, container_subtype);
                                            }
                                        }
                                    }

                                    service_address_total += container_total;
                                }

                                PE.DM.Address pe_service_address = await pedb.GetAddressById(service_address.PeaddressId);

                                Add_Bill_Service_Address_Record(pe_service_address, service_address, bill_service_address_records, bill_container_detail_records);

                                bill_service_address_records_inserted += 1;
                            }

                            if (!contract_charge_customer)
                            {
                                customer_total += service_address_total;
                            }
                        }
                    }

                    if (bill_service_address_records_inserted == 0 && service_address_records.Count > 0)
                    {
                        PE.DM.Address pe_service_address = await pedb.GetAddressById(service_address_records.OrderByDescending(s => s.CancelDate).ThenBy(s => s.LocationNumber).First().PeaddressId);

                        List<BillContainerDetail> bill_container_detail_records = new List<BillContainerDetail>();

                        Add_Bill_Service_Address_Record(pe_service_address, service_address_records.OrderByDescending(s => s.CancelDate).ThenBy(s => s.LocationNumber).First(), bill_service_address_records, bill_container_detail_records);

                        bill_service_address_records_inserted += 1;
                    }

                    PE.DM.PersonEntity pe_bill_master = await pedb.GetPersonEntityById(customer.Pe);

                    Transaction billing_transaction = Add_Monthly_Billing_Transaction_Record(customer, billing_transaction_code_records, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime, transaction_records);

                    await Add_Bill_Master_Record(pe_bill_master, customer, context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime, customer_total, bill_master_records, bill_service_address_records, billing_transaction);
                }
            }
        }

        int i = 0;
        foreach (var bill_master in bill_master_records)
        {
            i += 1;

            Console.WriteLine(String.Concat("Processing CBR | Bills Record #: ", i, " of ", bill_master_records.Count, " | Cust #: ", bill_master.CustomerId));

            swdb.BillMasters.Add(bill_master);
        }
        
        await swdbContext.SaveChangesAsync();

        List<Transaction> billing_summary_transaction_records = await swdb.GetTransactionsByAddDateTimeRange(context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime);

        decimal billing_summary_amt = billing_summary_transaction_records.Sum(t => t.TransactionAmt);

        Format_Billing_Summary_Email(context.Mthly_Bill_Beg_DateTime, context.Mthly_Bill_End_DateTime, billing_summary_transaction_records, billing_summary_amt);

        Console.WriteLine("Successful Job Completion");
    }

    public void Init_SW_Billing_Variables()
    {
        process_customer = false;
        process_location = false;
        process_container = false;

        balance_forward = 0;
        past_due_amt = 0;

        final_billing = false;

        credit_bal_final_billed_cancelled_acct = false;
        
        pos_bal_final_billed_cancelled_acct = false;

        past_due_bal_active_acct = false;
        past_due_bal_final_billed_cancelled_acct = false;
        past_due_bal_non_final_billed_cancelled_acct = false;

        contract_charge_customer = false;

        customer_total = 0;
        container_charge = 0;
        container_total = 0;
    }

    public async Task Eval_Cancelled_Customer(Customer customer, List<Transaction> transaction_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime) 
    {
        await Format_SW_Billing_Flags_For_Cancelled_Customer(customer, transaction_records, mthly_bill_beg_datetime, mthly_bill_end_datetime);
    }

    public async Task Eval_Active_Customer(Customer customer, List<Transaction> transaction_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        await Format_SW_Billing_Flags_For_Active_Customer(customer, transaction_records, mthly_bill_beg_datetime, mthly_bill_end_datetime);
    }

    public bool Eval_Contract_Charge_Customer(Customer customer)
    {
        if (customer.ContractCharge == null)
        {
            return contract_charge_customer = false;
        }
        else
        {
            return contract_charge_customer = true;
        }
    }

    public async Task Format_SW_Billing_Flags_For_Cancelled_Customer(Customer customer, List<Transaction> transaction_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        balance_forward = Get_Balance_Forward_For_Customer(transaction_records);

        past_due_amt = await Calculate_Past_Due_Amt_For_Customer(customer);

        bool final_billing_rec_found = Get_Final_Billing_Transaction_Record(customer, transaction_records, mthly_bill_beg_datetime, mthly_bill_end_datetime);

        if (Convert.ToDateTime(Convert.ToString(customer.CancelDate.GetValueOrDefault().Year.ToString().PadLeft(4, '0') + "-" + customer.CancelDate.GetValueOrDefault().Month.ToString().PadLeft(2, '0') + "-" + customer.CancelDate.GetValueOrDefault().Day.ToString().PadLeft(2, '0'))) >= Convert.ToDateTime(Convert.ToString(mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_beg_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_beg_datetime.Day.ToString().PadLeft(2, '0'))) && Convert.ToDateTime(Convert.ToString(customer.CancelDate.GetValueOrDefault().Year.ToString().PadLeft(4, '0') + "-" + customer.CancelDate.GetValueOrDefault().Month.ToString().PadLeft(2, '0') + "-" + customer.CancelDate.GetValueOrDefault().Day.ToString().PadLeft(2, '0'))) <= Convert.ToDateTime(Convert.ToString(mthly_bill_end_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_end_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_end_datetime.Day.ToString().PadLeft(2, '0'))))
        {
            //FINAL BILLING FOR CANCELLED ACCT

            final_billing = true;                                   

            process_customer = true;
        }

        if (balance_forward == 0 && final_billing_rec_found)
        {
            //ZERO BALANCE ON FINAL BILLED | CANCELLED ACCT
        }

        if (balance_forward == 0 && !final_billing_rec_found)
        {
            //ZERO BALANCE ON NON-FINAL BILLED | CANCELLED ACCT

            process_customer = true;
        }

        if (balance_forward < 0 && final_billing_rec_found)
        {
            //BILL REMINDER - CREDIT BALANCE ON FINAL BILLED | CANCELLED ACCT

            credit_bal_final_billed_cancelled_acct = true;          

            process_customer = true;
        }

        if (balance_forward < 0 && !final_billing_rec_found)
        {
            process_customer = true;
        }

        if (balance_forward > 0 && !final_billing_rec_found && past_due_amt <= 0)
        {
            //POSITIVE BALANCE ON NON-FINAL BILLED | CANCELLED ACCT - NO PAST DUE AMT       

            process_customer = true;
        }

        if (balance_forward > 0 && final_billing_rec_found && past_due_amt <= 0)
        {
            //BILL REMINDER - POSITIVE BALANCE ON FINAL BILLED | CANCELLED ACCT - NO PAST DUE AMT

            pos_bal_final_billed_cancelled_acct = true;             

            process_customer = true;
        }

        if (past_due_amt > 0 && final_billing_rec_found)
        {
            //BILL REMINDER - PAST DUE BALANCE ON FINAL BILLED | CANCELLED ACCT

            past_due_bal_final_billed_cancelled_acct = true;        

            process_customer = true;
        }

        if (past_due_amt > 0 && !final_billing_rec_found)
        {
            //PAST DUE BALANCE ON NON-FINAL BILLED | CANCELLED ACCT

            past_due_bal_non_final_billed_cancelled_acct = true;    

            process_customer = true;
        }

        if (balance_forward > 0 && !process_customer)
        {
            process_customer = true;
        }
    }

    public async Task Format_SW_Billing_Flags_For_Active_Customer(Customer customer, List<Transaction> transaction_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        balance_forward = Get_Balance_Forward_For_Customer(transaction_records);

        past_due_amt = await Calculate_Past_Due_Amt_For_Customer(customer);

        if (balance_forward == 0)
        {
            //ZERO BALANCE ON ACTIVE ACCT                                                       
        }

        if (balance_forward > 0 && past_due_amt <= 0)
        {
            //POSITIVE BALANCE ON ACTIVE ACCT - NO PAST DUE AMT                             
        }

        if (past_due_amt > 0)
        {
            //PAST DUE BALANCE ON ACTIVE ACCT

            past_due_bal_active_acct = true;                        
        }

        process_customer = true;
    }

    public static decimal Get_Balance_Forward_For_Customer(List<Transaction> transaction_records)
    {
        if (transaction_records.Count == 0)
        {
            return 0;
        }
        else
        {
            return transaction_records[0].TransactionBalance;
        }
    }

    public async Task<decimal> Calculate_Past_Due_Amt_For_Customer(Customer customer)
    {
        int days_past_due = 15;
        return await swdb.GetRemainingCurrentBalance(DateTime.Now, days_past_due, customer.CustomerId);
    }

    public static bool Get_Initial_Billing_Transaction_Record(Customer customer, List<Transaction> transaction_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        if (transaction_records.Count > 0)
        {
            var initial_billing_transaction_record = transaction_records.Where(t => (t.TransactionCode.Code == "MB" || t.TransactionCode.Code == "FB") && t.AddDateTime >= customer.EffectiveDate).OrderBy(t => t.AddDateTime).ThenBy(t => t.Sequence).FirstOrDefault();

            if (initial_billing_transaction_record != null)
            {
                return true;
            }
            else
            {
                if (Convert.ToDateTime(Convert.ToString(customer.EffectiveDate.Year.ToString().PadLeft(4, '0') + "-" + customer.EffectiveDate.Month.ToString().PadLeft(2, '0') + "-" + customer.EffectiveDate.Day.ToString().PadLeft(2, '0'))) < Convert.ToDateTime(Convert.ToString(mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_beg_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_beg_datetime.Day.ToString().PadLeft(2, '0'))))
                {
                    return true;     //Roll-Off and new customers initially will not have RF10 MB records.
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
    }

    public static bool Get_Final_Billing_Transaction_Record(Customer customer, List<Transaction> transaction_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        if (transaction_records.Count > 0)
        {
            var final_billed_transaction_record = transaction_records.Where(t => (t.TransactionCode.Code == "MB" || t.TransactionCode.Code == "FB") && t.AddDateTime > customer.CancelDate).OrderBy(t => t.AddDateTime).ThenBy(t => t.Sequence).FirstOrDefault();

            if (final_billed_transaction_record != null)
            {
                return true;
            }
            else
            {
                if (Convert.ToDateTime(Convert.ToString(customer.CancelDate.GetValueOrDefault().Year.ToString().PadLeft(4, '0') + "-" + customer.CancelDate.GetValueOrDefault().Month.ToString().PadLeft(2, '0') + "-" + customer.CancelDate.GetValueOrDefault().Day.ToString().PadLeft(2, '0'))) < Convert.ToDateTime(Convert.ToString(mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_beg_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_beg_datetime.Day.ToString().PadLeft(2, '0'))))
                {
                    return true;     //Roll-Off and new Customers initially will not have RF10 MB records.
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
    }

    public void Eval_Service_Address_Status(ServiceAddress service_address, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        if (Convert.ToDateTime(Convert.ToString(service_address.EffectiveDate.Year.ToString().PadLeft(4, '0') + "-" + service_address.EffectiveDate.Month.ToString().PadLeft(2, '0') + "-" + service_address.EffectiveDate.Day.ToString().PadLeft(2, '0'))) <= Convert.ToDateTime(Convert.ToString(mthly_bill_end_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_end_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_end_datetime.Day.ToString().PadLeft(2, '0'))))
        {
            if (service_address.CancelDate != null)
            {
                if (Convert.ToDateTime(Convert.ToString(service_address.CancelDate.Value.Year.ToString().PadLeft(4, '0') + "-" + service_address.CancelDate.Value.Month.ToString().PadLeft(2, '0') + "-" + service_address.CancelDate.Value.Day.ToString().PadLeft(2, '0'))) >= Convert.ToDateTime(Convert.ToString(mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_beg_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_beg_datetime.Day.ToString().PadLeft(2, '0'))))
                {
                    process_location = true;
                }
            }
            else
            {
                process_location = true;
            }
        }
    }

    public void Eval_Container_Status(Container container, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        if (Convert.ToDateTime(Convert.ToString(container.EffectiveDate.Year.ToString().PadLeft(4, '0') + "-" + container.EffectiveDate.Month.ToString().PadLeft(2, '0') + "-" + container.EffectiveDate.Day.ToString().PadLeft(2, '0'))) <= Convert.ToDateTime(Convert.ToString(mthly_bill_end_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_end_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_end_datetime.Day.ToString().PadLeft(2, '0'))))
        {
            if (container.CancelDate != null)
            {
                if (Convert.ToDateTime(Convert.ToString(container.CancelDate.Value.Year.ToString().PadLeft(4, '0') + "-" + container.CancelDate.Value.Month.ToString().PadLeft(2, '0') + "-" + container.CancelDate.Value.Day.ToString().PadLeft(2, '0'))) >= Convert.ToDateTime(Convert.ToString(mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_beg_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_beg_datetime.Day.ToString().PadLeft(2, '0'))) && container.CancelDate != container.EffectiveDate)
                {
                    process_container = true;
                }
            }
            else
            {
                process_container = true;
            }
        }
    }

    public async Task<decimal> Calculate_Container_Charge(Customer customer, ContainerCode container_code, Container container, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime, List<BillContainerDetail> bill_container_detail_records, ContainerSubtype container_subtype)
    {
        int container_prorated_num_days_service = 0;

        int number_of_days_service = Calculate_Number_Of_Days_Service(container);

        DateTime container_billing_beg_datetime = Eval_Container_Billing_Beg_DateTime(container, mthly_bill_beg_datetime, mthly_bill_end_datetime);
        DateTime container_billing_end_datetime = Eval_Container_Billing_End_DateTime(container, mthly_bill_beg_datetime, mthly_bill_end_datetime);

        List<ContainerRate> starting_container_rate_record = await swdb.GetContainerRateByCodeDaysSizeEffDate(container.ContainerCodeId, container.ContainerSubtypeId, number_of_days_service, container.BillingSize, container_billing_beg_datetime);

        if (starting_container_rate_record.Count == 0)
        {
            throw new InvalidOperationException(String.Concat("Could not find a container rate for Type: ", container_code.Type, ", ID: ", container.ContainerCodeId, ", SubType ID: ", container.ContainerSubtypeId, ", SubType Desc: ", container.ContainerSubtype.Description.ToString(), ", # Days Svc: ", number_of_days_service, ", Billing Size: ", container.BillingSize, " and Effective Date: ", container_billing_beg_datetime, "."));
        }

        container_prorated_num_days_service = (container_billing_end_datetime.DayOfYear - container_billing_beg_datetime.DayOfYear) + 1;

        if (container_subtype.BillingFrequency != "Weekly")
        {
            container_charge = (Math.Round((starting_container_rate_record[0].RateAmount * 1) / DateTime.DaysInMonth(Convert.ToInt32(context.Process_Date.AddMonths(-1).Year), Convert.ToInt32(context.Process_Date.AddMonths(-1).Month)) * container_prorated_num_days_service, 2)) + container.AdditionalCharge;
        }

        container_total += container_charge;

        Add_Bill_Container_Detail_Record(customer, container_code, container, starting_container_rate_record, 0, container_prorated_num_days_service, container_charge, bill_container_detail_records);
        
        return container_total;
    }

    public static int Calculate_Number_Of_Days_Service(Container container)
    {
        int number_of_days_service = 0;

        if (container.MonService)
        {
            number_of_days_service += 1;
        }

        if (container.TueService)
        {
            number_of_days_service += 1;
        }

        if (container.WedService)
        {
            number_of_days_service += 1;
        }

        if (container.ThuService)
        {
            number_of_days_service += 1;
        }

        if (container.FriService)
        {
            number_of_days_service += 1;
        }

        if (container.SatService)
        {
            number_of_days_service += 1;
        }

        return number_of_days_service;
    }

    public static DateTime Eval_Container_Billing_Beg_DateTime(Container container, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        DateTime container_billing_beg_datetime;

        if (Convert.ToDateTime(Convert.ToString(container.EffectiveDate.Year.ToString().PadLeft(4, '0') + "-" + container.EffectiveDate.Month.ToString().PadLeft(2, '0') + "-" + container.EffectiveDate.Day.ToString().PadLeft(2, '0'))) >= Convert.ToDateTime(Convert.ToString(mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_beg_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_beg_datetime.Day.ToString().PadLeft(2, '0'))))
        {
            container_billing_beg_datetime = container.EffectiveDate;
        }
        else
        {
            container_billing_beg_datetime = mthly_bill_beg_datetime;
        }

        return container_billing_beg_datetime;
    }

    public static DateTime Eval_Container_Billing_End_DateTime(Container container, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime)
    {
        DateTime container_billing_end_datetime;

        if (container.CancelDate != null)
        {
            if (Convert.ToDateTime(Convert.ToString(container.CancelDate.Value.Year.ToString().PadLeft(4, '0') + "-" + container.CancelDate.Value.Month.ToString().PadLeft(2, '0') + "-" + container.CancelDate.Value.Day.ToString().PadLeft(2, '0'))) < Convert.ToDateTime(Convert.ToString(mthly_bill_end_datetime.Year.ToString().PadLeft(4, '0') + "-" + mthly_bill_end_datetime.Month.ToString().PadLeft(2, '0') + "-" + mthly_bill_end_datetime.Day.ToString().PadLeft(2, '0'))))
            {
                container_billing_end_datetime = Convert.ToDateTime(container.CancelDate);
            }
            else
            {
                container_billing_end_datetime = mthly_bill_end_datetime;
            }
        }
        else
        {
            container_billing_end_datetime = mthly_bill_end_datetime;
        }

        return container_billing_end_datetime;
    }

    public static string Fmt_Days_Service_String(Container container)
    {
        string days_svc = string.Empty;

        if (container.MonService)
        {
            days_svc = String.Concat(days_svc, "M");
        }
        else
        {
            days_svc = string.Concat(days_svc, ".");
        }

        if (container.TueService)
        {
            days_svc = String.Concat(days_svc, "T");
        }
        else
        {
            days_svc = string.Concat(days_svc, ".");
        }

        if (container.WedService)
        {
            days_svc = String.Concat(days_svc, "W");
        }
        else
        {
            days_svc = string.Concat(days_svc, ".");
        }

        if (container.ThuService)
        {
            days_svc = String.Concat(days_svc, "T");
        }
        else
        {
            days_svc = string.Concat(days_svc, ".");
        }

        if (container.FriService)
        {
            days_svc = String.Concat(days_svc, "F");
        }
        else
        {
            days_svc = string.Concat(days_svc, ".");
        }

        if (container.SatService)
        {
            days_svc = String.Concat(days_svc, "S");
        }
        else
        {
            days_svc = string.Concat(days_svc, ".");
        }

        return days_svc;
    }

    public Task<string> Eval_Billing_Messages_For_Bills(Customer customer)
    {
        string sw_bills_msg = "";

        if (past_due_bal_final_billed_cancelled_acct)
        {
            sw_bills_msg = "BILL REMINDER - PAST DUE BALANCE ON FINAL BILLED | CANCELLED ACCT.";
        }

        if (past_due_bal_non_final_billed_cancelled_acct)
        {
            sw_bills_msg = "PAST DUE BALANCE ON NON-FINAL BILLED | CANCELLED ACCOUNT.";
        }

        if (pos_bal_final_billed_cancelled_acct)
        {
            sw_bills_msg = "BILL REMINDER - POSITIVE BALANCE ON FINAL BILLED | CANCELLED ACCT - NO PAST DUE AMT.";
        }

        if (past_due_bal_active_acct)
        {
            sw_bills_msg = "PAST DUE BALANCE ON ACTIVE ACCOUNT.";
        }

        if (sw_bills_msg == "")
        {
            if (credit_bal_final_billed_cancelled_acct)
            {
                sw_bills_msg = "BILL REMINDER - CREDIT BALANCE ON FINAL BILLED | CANCELLED ACCOUNT.";
            }
            else
            {
                if (contract_charge_customer)
                {
                    if (customer.CancelDate != null)
                    {
                        if (balance_forward + customer.ContractCharge.GetValueOrDefault() < 0)
                        {
                            sw_bills_msg = "CREDIT BALANCE ON NON-FINAL BILLED | CANCELLED ACCOUNT.";
                        }
                    }
                    else
                    {
                        if (balance_forward + customer.ContractCharge.GetValueOrDefault() < 0)
                        {
                            sw_bills_msg = "CREDIT BALANCE ON ACTIVE ACCOUNT.";
                        }
                    }
                }
                else
                {
                    if (customer.CancelDate != null)
                    {
                        if (balance_forward + customer_total < 0)
                        {
                            sw_bills_msg = "CREDIT BALANCE ON NON-FINAL BILLED | CANCELLED ACCOUNT.";
                        }
                    }
                    else
                    {
                        if (balance_forward + customer_total < 0)
                        {
                            sw_bills_msg = "CREDIT BALANCE ON ACTIVE ACCOUNT.";
                        }
                    }
                }
            }
        }

        return Task.FromResult(sw_bills_msg);
    }


    public async Task Add_Bill_Master_Record(PE.DM.PersonEntity pe_bill_master, Customer customer, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime, decimal customer_total, List<BillMaster> bill_master_records, List<BillServiceAddress> bill_service_address_records, Transaction bill_master_transaction)
    {
        BillMaster bill_master = new BillMaster();

        bill_master.CustomerType = customer.CustomerType;

        bill_master.CustomerId = customer.CustomerId;

        bill_master.ContractCharge = customer.ContractCharge;

        bill_master.BillingName = pe_bill_master.FullName;

        PE.DM.Address pe_billing_address = pe_bill_master.Addresses.SingleOrDefault(a => a.IsDefault);

        if (pe_billing_address != null)
        {
            bill_master.BillingAddressStreet = string.Concat(pe_billing_address.Number, " ", pe_billing_address.Direction, " ", pe_billing_address.StreetName, " ", pe_billing_address.Suffix, " ", pe_billing_address.Apt).Trim();

            bill_master.BillingAddressCityStateZip = string.Concat(pe_billing_address.City, " ", pe_billing_address.State, " ", pe_billing_address.Zip);
        }

        bill_master.BillingPeriodBegDate = mthly_bill_beg_datetime;

        bill_master.BillingPeriodEndDate = mthly_bill_end_datetime;

        bill_master.PastDueAmt = past_due_amt;

        if (customer.ContractCharge == null)
        {
            bill_master.ContainerCharges = customer_total;
        }
        else
        {
            bill_master.ContainerCharges = 0;
        }

        bill_master.FinalBill = final_billing;

        bill_master.BillMessage = await Eval_Billing_Messages_For_Bills(customer);

        bill_master.DeleteFlag = false;

        bill_master.AddDateTime = DateTime.Now;

        bill_master.AddToi = "Monthly Billing";

        bill_master.BillServiceAddresses = bill_service_address_records;

        bill_master.Transaction = bill_master_transaction;

        bill_master_records.Add(bill_master);
    }

    public void Add_Bill_Service_Address_Record(PE.DM.Address pe_service_address, ServiceAddress service_address, List<BillServiceAddress> bill_service_address_records, List<BillContainerDetail> bill_container_detail_records)
    {
        BillServiceAddress bill_service_address = new BillServiceAddress();

        bill_service_address.ServiceAddressId = service_address.Id;

        bill_service_address.ServiceAddressName = service_address.LocationName;

        bill_service_address.ServiceAddressStreet = string.Concat(pe_service_address.Number, " ", pe_service_address.Direction, " ", pe_service_address.StreetName, " ", pe_service_address.Suffix, " ", pe_service_address.Apt).Trim();

        bill_service_address.ServiceAddressCityStateZip = string.Concat(pe_service_address.City, " ", pe_service_address.State, " ", pe_service_address.Zip);

        bill_service_address.DeleteFlag = false;

        bill_service_address.AddDateTime = DateTime.Now;

        bill_service_address.AddToi = "Monthly Billing";

        bill_service_address.BillContainerDetails = bill_container_detail_records;

        bill_service_address_records.Add(bill_service_address);
    }
    
    public void Add_Bill_Container_Detail_Record(Customer customer, ContainerCode container_code, Container container, List<ContainerRate> container_rate_record, int i, int container_prorated_num_days_service, decimal container_charge, List<BillContainerDetail> bill_container_detail_records)
    {
        BillContainerDetail bill_container_detail = new BillContainerDetail();

        bill_container_detail.ContainerId = container.Id;

        bill_container_detail.ContainerType = container_code.Type;

        bill_container_detail.ContainerDescription = container_code.Description;

        bill_container_detail.ContainerEffectiveDate = container.EffectiveDate;

        bill_container_detail.ContainerCancelDate = container.CancelDate;

        bill_container_detail.RateAmount = container_rate_record[i].RateAmount;

        bill_container_detail.RateDescription = container_rate_record[i].RateDescription;

        bill_container_detail.DaysProratedMessage = string.Concat("PRORATED ", container_prorated_num_days_service, " OF ", DateTime.DaysInMonth(Convert.ToInt32(context.Process_Date.AddMonths(-1).Year), Convert.ToInt32(context.Process_Date.AddMonths(-1).Month)), " DAYS.");

        bill_container_detail.BillingSize = container.BillingSize;

        bill_container_detail.DaysService = Fmt_Days_Service_String(container);

        if (customer.ContractCharge == null)
        {
            bill_container_detail.ContainerCharge = container_charge;
        }
        else
        {
            bill_container_detail.ContainerCharge = 0;
        }

        bill_container_detail.ObjectCode = container_rate_record[i].ObjectCode;

        bill_container_detail.DeleteFlag = false;

        bill_container_detail.AddDateTime = DateTime.Now;

        bill_container_detail.AddToi = "Monthly Billing";

        bill_container_detail_records.Add(bill_container_detail);
    }

    public Transaction Add_Monthly_Billing_Transaction_Record(Customer customer, List<TransactionCode> billing_transaction_code_records, DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime, List<Transaction> transaction_records)
    {
        Transaction billing_transaction = new Transaction();

        billing_transaction.CustomerType = customer.CustomerType;

        billing_transaction.CustomerId = customer.CustomerId;

        if (!past_due_bal_final_billed_cancelled_acct && !credit_bal_final_billed_cancelled_acct && !pos_bal_final_billed_cancelled_acct)
        {
            if (final_billing)
            {
                billing_transaction.TransactionCodeId = billing_transaction_code_records[0].TransactionCodeId;

                billing_transaction.Comment = string.Concat(mthly_bill_beg_datetime.ToString("MMM").ToUpper(), " ", mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0'), " ", "FINAL BILL");
            }
            else
            {
                billing_transaction.TransactionCodeId = billing_transaction_code_records[1].TransactionCodeId;

                billing_transaction.Comment = string.Concat(mthly_bill_beg_datetime.ToString("MMM").ToUpper(), " ", mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0'), " ", "MONTHLY BILL");
            }

            if (contract_charge_customer)
            {
                billing_transaction.TransactionAmt = customer.ContractCharge.GetValueOrDefault();

                billing_transaction.TransactionBalance = balance_forward + customer.ContractCharge.GetValueOrDefault();
            }
            else
            {
                billing_transaction.TransactionAmt = customer_total;

                billing_transaction.TransactionBalance = balance_forward + customer_total;
            }
        }
        else
        {
            billing_transaction.TransactionCodeId = billing_transaction_code_records[2].TransactionCodeId;

            billing_transaction.Comment = string.Concat(mthly_bill_beg_datetime.ToString("MMM").ToUpper(), " ", mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0'), " ", "MONTHLY BILL REMINDER");

            billing_transaction.TransactionAmt = 0;

            billing_transaction.TransactionBalance = balance_forward;
        }

        if (transaction_records.Count == 0)
        {
            billing_transaction.CollectionsBalance = 0.00m;
            billing_transaction.CounselorsBalance = 0.00m;
            billing_transaction.UncollectableBalance = 0.00m;
        }
        else
        {
            billing_transaction.CollectionsBalance = transaction_records[0].CollectionsBalance;
            billing_transaction.CounselorsBalance = transaction_records[0].CounselorsBalance;
            billing_transaction.UncollectableBalance = transaction_records[0].UncollectableBalance;
        }

        billing_transaction.Sequence = 0;

        billing_transaction.DeleteFlag = false;

        billing_transaction.AddDateTime = DateTime.Now;

        billing_transaction.AddToi = "Monthly Billing";

        return billing_transaction;
    }

    public void Format_Billing_Summary_Email(DateTime mthly_bill_beg_datetime, DateTime mthly_bill_end_datetime, List<Transaction> billing_summary_transaction_records, decimal billing_summary_amt)
    {
        context.BillingSummaryWriter.WriteLine("Total charges and fees for " + mthly_bill_beg_datetime.ToString("MMM") + " " + mthly_bill_beg_datetime.Year.ToString().PadLeft(4, '0') + " : " + Convert.ToDecimal(billing_summary_amt));
    }
}
