using Common.Services.TelerikReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PE.DAL.Contexts;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BillBlobs.Services;

/// <summary>
/// Runs every 1st of the month (6:30 AM) after SW.Billing and SW.BillGenerate
/// </summary>

public class BillBlobGenerateService
{
    private readonly IDbContextFactory<SwDbContext> swdbFactory;
    private readonly IDbContextFactory<PeDbContext> pedbFactory;

    private readonly IReportingService _reportingService;
    private readonly ReportingServiceOptions _options;

    private SwDbContext swdb;
    private PeDbContext pedb;

    private BillBlobContext context;

    public BillBlobGenerateService(IDbContextFactory<SwDbContext> swdbFactory, IDbContextFactory<PeDbContext> pedbFactory, IReportingService reportingService, IOptions<ReportingServiceOptions> options)
    {
        this.swdbFactory = swdbFactory;
        this.pedbFactory = pedbFactory;

        this._reportingService = reportingService;
        _options = options.Value;
    }

    public async Task Handle(BillBlobContext context)
    {
        using var swdbContext = swdbFactory.CreateDbContext();
        swdb = swdbContext;

        using var pedbContext = pedbFactory.CreateDbContext();
        pedb = pedbContext;

        this.context = context;

        await Process(swdbContext);
    }

    public async Task Process(SwDbContext swdbContext)
    {
        context.Mthly_Bill_Blob_Beg_DateTime = await swdb.Get_Last_Bill_Tran_DateTime_For_Prev_Billing(context.Process_Date_Beg);
        context.Mthly_Bill_Blob_End_DateTime = await swdb.Get_Last_Bill_Tran_DateTime_For_Curr_Billing(context.Process_Date_Beg);

        List<BillMaster> bill_master_records = await swdb.GetBillMasterRecordsByDateRange(context.Process_Date_Beg, context.Process_Date_End);
        if (!bill_master_records.Any()) 
        {
            throw new InvalidOperationException("No Bill Master records found for the monthly billing period of " + string.Concat(context.Mthly_Bill_Blob_Beg_DateTime.Year.ToString().PadLeft(4, '0'), "-", context.Mthly_Bill_Blob_Beg_DateTime.Month.ToString().PadLeft(2, '0'), "-", context.Mthly_Bill_Blob_Beg_DateTime.Day.ToString().PadLeft(2, '0')) + " thru " + string.Concat(context.Mthly_Bill_Blob_End_DateTime.Year.ToString().PadLeft(4, '0'), "-", context.Mthly_Bill_Blob_End_DateTime.Month.ToString().PadLeft(2, '0'), "-", context.Mthly_Bill_Blob_End_DateTime.Day.ToString().PadLeft(2, '0')) + ".");
        }

        foreach(var bill in bill_master_records) 
        {
            Console.WriteLine("Processing Account " + bill.CustomerId);

            //call the report server api to generate batch pdf sw bill blob file for online viewing.
            var parameters = new Dictionary<string, object>
            {
                {"beg_datetime", context.Mthly_Bill_Blob_Beg_DateTime.ToString("MM/dd/yyyy HH:mm:ss.FFF")},
                {"end_datetime", context.Mthly_Bill_Blob_End_DateTime.ToString("MM/dd/yyyy HH:mm:ss.FFF")},
                {"customer_id", bill.CustomerId},
                //{"connectionString", _options.ConnectionString}
            };
            var byte_array = await _reportingService.GenerateReportPDF("SW_Bill", parameters);

            SW.DM.BillBlobs bill_blob = new SW.DM.BillBlobs();

            bill_blob.BillMasterId = bill.BillMasterId;
            bill_blob.CustomerId = bill.CustomerId;
            bill_blob.TransactionId = bill.TransactionId;
            bill_blob.BegDateTime = context.Mthly_Bill_Blob_Beg_DateTime;
            bill_blob.EndDateTime = context.Mthly_Bill_Blob_End_DateTime;
            bill_blob.BillFile = byte_array;
            bill_blob.AddToi = "BAT";
            bill_blob.AddDateTime = DateTime.Now;

            swdb.BillBlobs.Add(bill_blob);
        }

        await swdbContext.SaveChangesAsync();

        context.BillBlobSummaryWriter.WriteLine(bill_master_records.Count + " Bill Blobs processed for " + context.Mthly_Bill_Blob_Beg_DateTime.ToString("MMM") + " " + context.Mthly_Bill_Blob_Beg_DateTime.Year.ToString().PadLeft(4, '0') + ".");

        Console.WriteLine("Successful Job Completion");
    }
}
