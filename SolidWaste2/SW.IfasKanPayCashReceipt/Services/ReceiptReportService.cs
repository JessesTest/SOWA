using SW.DAL.Contexts;

namespace SW.IfasKanPayCashReceipt.Services;

public sealed class ReceiptReportService
{
    private readonly string payment_code = "CA";

    private readonly SwDbContext db;
    
    private ReceiptContext context;

    public ReceiptReportService(SwDbContext db)
    {
        this.db = db;
    }

    public async Task Handle(ReceiptContext context)
    {
        this.context = context;

        decimal Commercial_Accum = 0.00m;
        decimal Residential_Accum = 0.00m;
        decimal RollOff_Accum = 0.00m;
        decimal Bulky_Accum = 0.00m;
        decimal Late_Accum = 0.00m;
        decimal Collection_Accum = 0.00m;

        var updated_charge_transactions = await db
            .GetUpdatedChargeTransactionsByAddDateTimeRangeForCashReceipt(
            context.CashReceiptBeginDatetime,
            DateTime.Now);

        foreach (var charge_tran in updated_charge_transactions)
        {
            if (charge_tran.ObjectCode == 43401)    //commercial 
            {
                Commercial_Accum += Convert.ToDecimal(charge_tran.Partial);
            }
            else if (charge_tran.ObjectCode == 43402)    //residential 
            {
                Residential_Accum += Convert.ToDecimal(charge_tran.Partial);
            }
            else if (charge_tran.ObjectCode == 43403)    //rolloff
            {
                RollOff_Accum += Convert.ToDecimal(charge_tran.Partial);
            }
            else if (charge_tran.ObjectCode == 43201)    //bulky
            {
                Bulky_Accum += Convert.ToDecimal(charge_tran.Partial);
            }
            else if (charge_tran.ObjectCode == 41601)    //late 
            {
                Late_Accum += Convert.ToDecimal(charge_tran.Partial);
            }
            else if (charge_tran.ObjectCode == 46202)    //collection 
            {
                Collection_Accum += Convert.ToDecimal(charge_tran.Partial);
            }
        }


        var updated_container_detail = await db
            .GetUpdatedContainerDetailByAddDateTimeRangeForCashReceipt(
            context.CashReceiptBeginDatetime,
            DateTime.Now);

        foreach (var container_detail in updated_container_detail)
        {
            if (container_detail.ObjectCode == 43401)    //commercial 
            {
                Commercial_Accum += Convert.ToDecimal(container_detail.Partial);
            }
            else if (container_detail.ObjectCode == 43402)    //residential 
            {
                Residential_Accum += Convert.ToDecimal(container_detail.Partial);
            }
            else if (container_detail.ObjectCode == 43403)    //rolloff
            {
                RollOff_Accum += Convert.ToDecimal(container_detail.Partial);
            }
            else if (container_detail.ObjectCode == 43201)    //bulky
            {
                Bulky_Accum += Convert.ToDecimal(container_detail.Partial);
            }
            else if (container_detail.ObjectCode == 41601)    //late 
            {
                Late_Accum += Convert.ToDecimal(container_detail.Partial);
            }
            else if (container_detail.ObjectCode == 46202)    //collection 
            {
                Collection_Accum += Convert.ToDecimal(container_detail.Partial);
            }
        }

        var lines = new[]
        {
            new ReportLine("43401", Commercial_Accum),
            new ReportLine("43201", Bulky_Accum),
            new ReportLine("43402", Residential_Accum),
            new ReportLine("43403", RollOff_Accum),
            new ReportLine("46202", Collection_Accum),
            new ReportLine("41601", Late_Accum)
        };
        foreach (var line in lines)
        {
            Generate_IFAS_File(line);
        }
    }

    private sealed record ReportLine(string Code, decimal Total);

    private void Generate_IFAS_File(ReportLine reportLine)
    {
        var obj_code = reportLine.Code;
        var obj_total = reportLine.Total;
        var cash_receipt_for_date = context.CashReceiptForDate;

        if (obj_total <= 0)
            return;


        string cash_rcpt = String.Concat(
        "CR",
        Convert.ToString("C000055").PadRight(12, ' '),
        string.Empty.PadRight(30, ' '),
        "GL",
        Convert.ToString("25SW000").PadRight(10, ' '),
        obj_code.PadRight(8, ' '),
        string.Empty.PadRight(2, ' '),
        string.Empty.PadRight(10, ' '),
        string.Empty.PadRight(8, ' '),
        string.Empty.PadRight(12, ' '),
        Convert.ToString("T").PadRight(8, ' '),
        "GEN ",
        string.Empty.PadRight(8, ' '),
        string.Empty.PadLeft(10, '0'),
        //string.Concat("Cash Receipt for ", DateTime.Now.Month.ToString().PadLeft(2, '0'), "/", DateTime.Now.Day.ToString().PadLeft(2, '0'), "/", DateTime.Now.Year.ToString().PadLeft(4, '0'), ".").PadRight(30, ' '),
        string.Concat("Cash Receipt for ", cash_receipt_for_date.Month.ToString().PadLeft(2, '0'), "/", cash_receipt_for_date.Day.ToString().PadLeft(2, '0'), "/", cash_receipt_for_date.Year.ToString().PadLeft(4, '0'), ".").PadRight(30, ' '),
        //string.Concat(Convert.ToString(DateTime.Now.Year).PadLeft(4, '0'), Convert.ToString(DateTime.Now.Month).PadLeft(2, '0'), Convert.ToString(DateTime.Now.Day).PadLeft(2, '0')),
        string.Concat(Convert.ToString(cash_receipt_for_date.Year).PadLeft(4, '0'), Convert.ToString(cash_receipt_for_date.Month).PadLeft(2, '0'), Convert.ToString(cash_receipt_for_date.Day).PadLeft(2, '0')),
        string.Empty.PadRight(8, ' '),
        string.Empty.PadRight(4, ' '),
        Convert.ToString("1").PadLeft(8, '0'),
        Convert.ToString(obj_total).PadLeft(12, '0'),
        payment_code,
        string.Empty.PadRight(10, ' '),
        "AP",
        Convert.ToString("BANK99").PadRight(10, ' '),
        //string.Concat(Convert.ToString(DateTime.Now.Year).PadLeft(4, '0'), Convert.ToString(DateTime.Now.Month).PadLeft(2, '0'), Convert.ToString(DateTime.Now.Day).PadLeft(2, '0')),
        string.Concat(Convert.ToString(cash_receipt_for_date.Year).PadLeft(4, '0'), Convert.ToString(cash_receipt_for_date.Month).PadLeft(2, '0'), Convert.ToString(cash_receipt_for_date.Day).PadLeft(2, '0')),
        string.Empty.PadRight(2, ' '),
        string.Empty.PadRight(2, ' '),
        "NB",
        "N");
        
        context.ReportWriter.WriteLine(cash_rcpt);
    }

}
