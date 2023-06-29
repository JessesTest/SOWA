using SW.DM;

namespace SW.IfasCashReceipt.Services;

public sealed class CashReceiptUpdateService
{
    private CashReceiptContext Context;

    private readonly ContainerRateRepository containerRateRepository;
    private readonly TransactionRepository transactionRepository;
    private readonly BillContainerDetailRepository billContainerDetailRepository;

    public CashReceiptUpdateService(
        ContainerRateRepository containerRateRepository,
        TransactionRepository transactionRepository,
        BillContainerDetailRepository billContainerDetailRepository)
    {
        this.containerRateRepository = containerRateRepository;
        this.transactionRepository = transactionRepository;
        this.billContainerDetailRepository = billContainerDetailRepository;
    }

    public Task Handle(CashReceiptContext context)
    {
        Context = context;
        return DoUpdates();
    }

    private async Task DoUpdates()
    {
        var allPayments = await transactionRepository.GetPaymentsByAddDateTimeRange(
            Context.CashReceiptForDate,
            Context.CashReceiptForDate.AddDays(1));

        Context.TotalPaymentsFound = allPayments.Count;

        var customers = allPayments.Select(p => p.Customer).Distinct().ToArray();
        foreach (var customer in customers)
        {
            var customerPayments = allPayments.Where(p => p.CustomerId == customer.CustomerId).ToList();
            await ProcessCustomerPayments(customer, customerPayments);
        }

        await transactionRepository.SaveChangesAsync();
        await billContainerDetailRepository.SaveChangesAsync();
    }

    public async Task ProcessCustomerPayments(
        Customer customer,
        ICollection<Transaction> payments)
    {
        if (!payments.Any())
        {
            Console.WriteLine($"No Payment Transactions Records for Cust #: {customer.CustomerId}"); // ???
            return;
        }

        // payment transaction amounts are all negative
        var Payment_Amt = Math.Abs(payments.Sum(t => t.TransactionAmt));

        var unpaidChargesForCustomer = await transactionRepository.GetUnpaidChargesForCustomer(customer);
        if (unpaidChargesForCustomer.Any())
        {
            var last_charge_record = 0;
            foreach (var charge in unpaidChargesForCustomer)
            {
                last_charge_record++;

                Console.WriteLine($"Processing Charge Transactions Record #: {last_charge_record} of {unpaidChargesForCustomer.Count} | Cust #: {customer.CustomerId} Code {charge.TransactionCode.Code}");

                Payment_Amt = await ProcessCustomerChargeRecord(
                    last_charge_record == unpaidChargesForCustomer.Count,
                    charge,
                    Payment_Amt);
            }
            if (Payment_Amt > 0)
            {
                Context.ErrorWriter.WriteLine("No Charge Transaction for 2  " + customer.CustomerId + "  " + Payment_Amt);
            }
        }
        if (Payment_Amt > 0)
        {
            Context.ErrorWriter.WriteLine("No Charge Transaction for 1  " + customer.CustomerId + "  " + Payment_Amt);
        }
    }

    private async Task<decimal> ProcessCustomerChargeRecord(
        bool final,
        Transaction charge,
        decimal Payment_Amt)
    {
        if (Payment_Amt <= 0)
            return Payment_Amt;

        if (charge.TransactionCode.Code == "MB" || charge.TransactionCode.Code == "FB")
        {
            if (charge.Customer.ContractCharge > 0)
            {
                return await Update_Transaction_Object_Code(charge, final, Payment_Amt);
            }
            else
            {
                return await Update_Bill_Container_Details_Object_Code(charge, final, Payment_Amt);
            }
        }
        else
        {
            return await Update_Transaction_Object_Code(charge, final, Payment_Amt);
        }
    }

    private async Task<decimal> Update_Transaction_Object_Code(Transaction charge, bool final, decimal Payment_Amt)
    {
        if (final)
        {
            if (Payment_Amt >= charge.TransactionAmt)
            {
                charge.PaidFull = true;
                charge.Partial = Payment_Amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Last Transaction for ");
                Payment_Amt = 0;
            }
            else
            {
                charge.PaidFull = false;
                charge.Partial = Payment_Amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Last Partial Trans   ");
                Payment_Amt = 0;
            }
        }
        else
        {
            if (Payment_Amt >= charge.TransactionAmt)
            {
                charge.PaidFull = true;
                charge.Partial = charge.TransactionAmt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                Payment_Amt -= charge.TransactionAmt;
                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Full Paid Trans  ");
            }
            else
            {
                charge.PaidFull = false;
                charge.Partial = Payment_Amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Partial Trans   ");
                Payment_Amt = 0;
            }
        }
        return Payment_Amt;
    }

    private async Task<int> Determine_Transaction_Object_Code(Transaction charge)
    {
        Console.WriteLine("Transaction Object Code customer: " + charge.CustomerId + " Code: " + charge.TransactionCode);

        switch (charge.TransactionCode.Code)
        {
            case "WRC":
            case "WRO":
            case "WRR":
            case "BK":
            case "EP":
            case "RD":
            case "RR":
            case "RF":
            case "LF":
            case "CF":
            case "WC":
            case "CR": //SCMB-252-Container-Reset-Billing-Code
                if (charge.ObjectCode > 0)
                {
                    return charge.ObjectCode;
                }

                if (charge.TransactionCode.Code == "WRC" || charge.TransactionCode.Code == "EP")
                {
                    return 43401;
                }

                if (charge.TransactionCode.Code == "WRO" || charge.TransactionCode.Code == "RD" || charge.TransactionCode.Code == "RR" || charge.TransactionCode.Code == "RF" || charge.TransactionCode.Code == "WC")
                {
                    return await Determine_Object_Code(charge);
                }

                if (charge.TransactionCode.Code == "WRR")
                {
                    return await Determine_Object_Code(charge);
                }

                //SCMB-252-Container-Reset-Billing-Code
                if (charge.TransactionCode.Code == "BK" || charge.TransactionCode.Code == "CR")
                {
                    return 43201;
                }

                if (charge.TransactionCode.Code == "LF")
                {
                    return 41601;
                }

                if (charge.TransactionCode.Code == "CF")
                {
                    return 46202;
                }
                return 43401;
            default:
                return 43401;
        }
    }

    private async Task<int> Determine_Object_Code(Transaction charge)
    {
        if (charge.Container == null)
        {
            return 43403;
        }
        var c = charge.Container;
        var container_code = c.ContainerCode;
        int number_of_days_service = Calculate_Number_Of_Days_Service(c);

        //SCMB-243-New-Container-Rates-For-2022
        DateTime effective_date = DateTime.Today;
        if (c.EffectiveDate > DateTime.Today)
        {
            effective_date = c.EffectiveDate;
        }

        List<ContainerRate> container_rate_record =
            await containerRateRepository.GetContainerRateByCodeDaysSizeEffDate(
                c.ContainerCodeId,
                c.ContainerSubtypeId,
                number_of_days_service,
                c.BillingSize,
                effective_date);

        if (!container_rate_record.Any())
        {
            Context.ErrorWriter.WriteLine(
                "Could not find a container rate record for Type: " + container_code.Type + ", ID: " + c.ContainerCodeId + ", SubType ID: " + c.ContainerSubtypeId + ", SubType Desc: " + c.ContainerSubtype.Description.ToString() + ", # Days Svc: " + number_of_days_service + ", Billing Size: " + c.BillingSize + " and Effective Date: " + Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")) + ".");

            return 43401;
        }
        else
        {
            if (container_rate_record.First().RateDescription.Contains("GAL") || container_rate_record.First().BillingSize <= 8)
            {
                return 43401;
            }
            else
            {
                return 43403;
            }
        }
    }

    private async Task<decimal> Update_Bill_Container_Details_Object_Code(
        Transaction charge,
        bool finalCharge,
        decimal Payment_Amt)
    {
        if (charge.TransactionCode.Code == "MB" || charge.TransactionCode.Code == "FB")
        {
            var bill_container_detail = await billContainerDetailRepository.GetByTransaction(charge.Id);

            if (bill_container_detail.Count == 0 && finalCharge)
            {
                charge.PaidFull = true;
                charge.Partial = Payment_Amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                Payment_Amt -= Payment_Amt;
                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " No CBR Transaction!");
            }
            else
            {
                int last_container_record = 0;

                foreach (var container in bill_container_detail)
                {
                    last_container_record += 1;

                    var dbcont = await billContainerDetailRepository.GetFirstUnpaid(charge.Id, container.ContainerId);

                    if (dbcont != null)
                    {
                        if (last_container_record == bill_container_detail.Count && finalCharge)
                        {
                            if (Payment_Amt >= container.ContainerCharge)
                            {
                                dbcont.PaidFull = true;
                                dbcont.Partial = Payment_Amt;
                                dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                                dbcont.ChgDateTime = DateTime.Now;
                                dbcont.ChgToi = "Cash Receipt";
                                Payment_Amt = 0;
                                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Last Paid Full CBR for ");
                            }
                            else
                            {
                                dbcont.PaidFull = false;
                                dbcont.Partial = Payment_Amt;
                                dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                                dbcont.ChgDateTime = DateTime.Now;
                                dbcont.ChgToi = "Cash Receipt";
                                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Last Partial Pay CBR for ");
                                Payment_Amt = 0;
                            }
                        }
                        else
                        {
                            if (Payment_Amt >= container.ContainerCharge)
                            {
                                dbcont.PaidFull = true;
                                dbcont.Partial = container.ContainerCharge;
                                dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                                dbcont.ChgDateTime = DateTime.Now;
                                dbcont.ChgToi = "Cash Receipt";
                                Payment_Amt -= container.ContainerCharge;
                                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Full Pay CBR for ");
                            }
                            else
                            {
                                dbcont.PaidFull = false;
                                dbcont.Partial = Payment_Amt;
                                dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                                dbcont.ChgDateTime = DateTime.Now;
                                dbcont.ChgToi = "Cash Receipt";
                                Context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Partial Pay CBR for ");
                                Payment_Amt = 0;
                            }
                        }
                    }
                    else if (charge.PaidFull == false)
                    {
                        charge.PaidFull = true;
                        charge.Partial = Payment_Amt;
                        charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                        charge.ChgDateTime = DateTime.Now;
                        charge.ChgToi = "Cash Receipt";
                        Payment_Amt -= Payment_Amt;
                        Context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " No CBR Transaction!");
                    }
                }
            }
        }

        return Payment_Amt;
    }

    private static int Calculate_Number_Of_Days_Service(Container container)
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

    private static int Determine_Bill_Container_Detail_Object_Code(BillContainerDetail container)
    {
        Console.WriteLine("Container Object Code code: " + container.ContainerType);

        switch (container.ContainerType)
        {
            case "C":
            case "M":
            case "O":
            case "R":
            case "S":
            case "Z":
                {
                    if (container.ObjectCode > 0)
                    {
                        return container.ObjectCode;
                    }

                    if (container.ContainerType == "C")
                    {
                        return 43401;
                    }

                    if (container.ContainerType == "M")
                    {
                        return 43401;
                    }

                    if (container.ContainerType == "O")
                    {
                        return 43403;
                    }

                    if (container.ContainerType == "R")
                    {
                        return 43402;
                    }

                    if (container.ContainerType == "S")
                    {
                        return 43201;
                    }

                    if (container.ContainerType == "Z")
                    {
                        return 43402;
                    }

                    return 43401;
                }
        }
        return 43401;
    }

}
