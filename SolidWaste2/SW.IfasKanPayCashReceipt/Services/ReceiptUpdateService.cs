using SW.DAL.Contexts;
using SW.DM;

namespace SW.IfasKanPayCashReceipt.Services;

public sealed class ReceiptUpdateService
{
    private readonly SwDbContext db;

    private ReceiptContext context;

    public ReceiptUpdateService(SwDbContext db)
    {
        this.db = db;
    }

    public async Task Handle(ReceiptContext context)
    {
        this.context = context;

        var customers = await db.GetAllCustomers();
        foreach(var customer in customers)
        {
            await ProcessCustomer(customer);
        }
        await db.SaveChangesAsync();

    }

    private async Task ProcessCustomer(Customer customer)
    {
        var transaction_pay_records = await db.GetTransactionsByAddDateTimeRangeForKPCashReceipt(
            context.CashReceiptForDate,
            context.CashReceiptForDate.AddDays(1),
            customer.CustomerId);

        context.TotalPaymentsFound += transaction_pay_records.Count;

        if (!transaction_pay_records.Any())
        {
            Console.WriteLine("No Payment Transactions Records for Cust #: " + customer.CustomerId);
            return;
        }

        // paymnet records have negative transaction amt
        decimal payment_amt = Math.Abs(transaction_pay_records.Sum(t => t.TransactionAmt));

        var transaction_charge_records = await db.GetTransactionsChargesForCustomer(customer.CustomerId);

        context.TotalChargesFound += transaction_charge_records.Count;

        if (!transaction_charge_records.Any())
        {
            if (payment_amt > 0)
            {
                context.ErrorWriter.WriteLine("No Charge Transaction for 1  " + customer.CustomerId + "  " + payment_amt);
            }
            return;
        }

        int last_charge_record = 0;
        foreach(var charge in transaction_charge_records)
        {
            last_charge_record += 1;
            Console.WriteLine("Processing Charge Transactions Record #: " + last_charge_record + " of " + transaction_charge_records.Count + " | Cust #: " + customer.CustomerId + " Code " + charge.TransactionCode);

            if(payment_amt <= 0)
            {
                continue;   // ?
            }

            if (charge.TransactionCode.Code == "MB" || charge.TransactionCode.Code == "FB")
            {
                if (customer.ContractCharge > 0)
                {
                    payment_amt = await Update_Transaction_Object_Code(charge, transaction_charge_records.Count == last_charge_record, payment_amt);
                }
                else
                {
                    payment_amt = await Update_Bill_Container_Details_Object_Code(charge, transaction_charge_records.Count == last_charge_record, payment_amt);
                }
            }
            else
            {
                payment_amt = await Update_Transaction_Object_Code(charge, transaction_charge_records.Count == last_charge_record, payment_amt);
            }
        }


        if (payment_amt > 0)
        {
            context.ErrorWriter.WriteLine("No Charge Transaction for 2  " + customer.CustomerId + "  " + payment_amt);
        }
    }

    private async Task<decimal> Update_Transaction_Object_Code(
        Transaction charge,
        bool final,
        decimal payment_amt)
    {
        if (final)
        {
            if (payment_amt >= charge.TransactionAmt)
            {
                charge.PaidFull = true;
                charge.Partial = payment_amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Last Transaction for ");
                payment_amt = 0;
                //Payment_Amt -= charge.TransactionAmt;
            }
            else
            {
                charge.PaidFull = false;
                charge.Partial = payment_amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Last Partial Trans   ");
                payment_amt = 0;
            }
        }
        else
        {
            if (payment_amt >= charge.TransactionAmt)
            {
                charge.PaidFull = true;
                charge.Partial = charge.TransactionAmt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                payment_amt -= charge.TransactionAmt;
                context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Full Paid Trans  " + Environment.NewLine);
            }
            else
            {
                charge.PaidFull = false;
                charge.Partial = payment_amt;
                charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                charge.ChgDateTime = DateTime.Now;
                charge.ChgToi = "Cash Receipt";
                context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " Partial Trans   " + Environment.NewLine);
                payment_amt = 0;
            }
        }
        return payment_amt;
    }

    private async Task<int> Determine_Transaction_Object_Code(Transaction charge)
    {
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
                    int object_code = await Determine_Object_Code(charge);
                    return object_code;
                }

                if (charge.TransactionCode.Code == "WRR")
                {
                    int object_code = await Determine_Object_Code(charge);
                    return object_code;
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
        if(charge.ContainerId is null)
        {
            return 43403;
        }

        var container = await db.GetContainer(charge.ContainerId.Value);
        var container_code = container.ContainerCode;
        int number_of_days_service = Calculate_Number_Of_Days_Service(container);

        //SCMB-243-New-Container-Rates-For-2022
        DateTime effective_date = DateTime.Today;
        if (container.EffectiveDate > DateTime.Today)
        {
            effective_date = container.EffectiveDate;
        }

        var container_rate_record = await db.GetContainerRate(
            container.ContainerCodeId,
            container.ContainerSubtypeId,
            number_of_days_service,
            container.BillingSize,
            effective_date);

        if(container_rate_record == null)
        {
            context.ErrorWriter.WriteLine(
                "Could not find a container rate record for Type: " + container_code.Type + 
                ", ID: " + container.ContainerCodeId + 
                ", SubType ID: " + container.ContainerSubtypeId + 
                ", SubType Desc: " + container.ContainerSubtype.Description.ToString() + 
                ", # Days Svc: " + number_of_days_service + 
                ", Billing Size: " + container.BillingSize + 
                " and Effective Date: " + Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")) + 
                ".");

            return 43401;
        }

        if(container_rate_record.RateDescription.Contains("GAL") || container_rate_record.BillingSize <= 8)
        {
            return 43401;
        }

        return 43403;
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

    private async Task<decimal> Update_Bill_Container_Details_Object_Code(Transaction charge, bool finalCharge, decimal payment_amt)
    {
        if (!(charge.TransactionCode.Code == "MB" || charge.TransactionCode.Code == "FB"))
            return payment_amt;

        var bill_container_detail = await db.Get_Bill_Container_Detail_For_Cash_Receipt(charge.Id);

        if (bill_container_detail.Count == 0 && finalCharge)
        {
            charge.PaidFull = true;
            charge.Partial = payment_amt;
            charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
            charge.ChgDateTime = DateTime.Now;
            charge.ChgToi = "Cash Receipt";
            payment_amt -= payment_amt;
            context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " No CBR Transaction!");
        }
        else
        {
            int last_container_record = 0;
            foreach (var container in bill_container_detail)
            {
                last_container_record += 1;

                //var dbcont = db.BillContainerDetail.Where(t => t.BillServiceAddress.BillMaster.TransactionID == charge.Id && t.ContainerId == container.ContainerId && t.PaidFull != true).FirstOrDefault();
                var dbcont = await db.GetBillContainerDetails(charge.Id, container.ContainerId);

                if (dbcont != null)
                {
                    if (last_container_record == bill_container_detail.Count && finalCharge)
                    {
                        if (payment_amt >= container.ContainerCharge)
                        {
                            dbcont.PaidFull = true;
                            dbcont.Partial = payment_amt;
                            dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                            dbcont.ChgDateTime = DateTime.Now;
                            dbcont.ChgToi = "Cash Receipt";
                            payment_amt = 0;
                            context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Last Paid Full CBR for ");
                        }
                        else
                        {
                            dbcont.PaidFull = false;
                            dbcont.Partial = payment_amt;
                            dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                            dbcont.ChgDateTime = DateTime.Now;
                            dbcont.ChgToi = "Cash Receipt";
                            context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Last Partial Pay CBR for ");
                            payment_amt = 0;
                        }
                    }
                    else
                    {
                        if (payment_amt >= container.ContainerCharge)
                        {
                            dbcont.PaidFull = true;
                            dbcont.Partial = container.ContainerCharge;
                            dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                            dbcont.ChgDateTime = DateTime.Now;
                            dbcont.ChgToi = "Cash Receipt";
                            payment_amt -= container.ContainerCharge;
                            context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Full Pay CBR for ");
                        }
                        else
                        {
                            dbcont.PaidFull = false;
                            dbcont.Partial = payment_amt;
                            dbcont.ObjectCode = Determine_Bill_Container_Detail_Object_Code(container);
                            dbcont.ChgDateTime = DateTime.Now;
                            dbcont.ChgToi = "Cash Receipt";
                            context.GoodWriter.WriteLine(charge.CustomerId + "  " + dbcont.Partial + "  " + dbcont.ObjectCode + " Partial Pay CBR for ");
                            payment_amt = 0;
                        }
                    }
                }
                else if (charge.PaidFull == false)
                {
                    charge.PaidFull = true;
                    charge.Partial = payment_amt;
                    charge.ObjectCode = await Determine_Transaction_Object_Code(charge);
                    charge.ChgDateTime = DateTime.Now;
                    charge.ChgToi = "Cash Receipt";
                    payment_amt -= payment_amt;
                    context.GoodWriter.WriteLine(charge.CustomerId + "  " + charge.Partial + "  " + charge.ObjectCode + " No CBR Transaction!");
                }
            }
        }
        return payment_amt;
    }

    public int Determine_Bill_Container_Detail_Object_Code(BillContainerDetail container)
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
