using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using PE.DAL.Contexts;
using SW.DM;
using PE.DM;
using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.Graph;
using Common.Services.TelerikReporting;
using Microsoft.Extensions.Options;
using Telerik.Reporting.Cache.File;
using Telerik.Reporting.Services;

namespace SW.BillGenerate.Services;

/// <summary>
/// Runs every 1st of the month (6:30 AM) after SW.Billing
/// </summary>

public class BillGenerateBatchPdfService
{
    private readonly IDbContextFactory<SwDbContext> swdbFactory;
    private readonly IDbContextFactory<PeDbContext> pedbFactory;

    private readonly IReportingService _reportingService;
    private readonly ReportingServiceOptions _options;

    private SwDbContext swdb;
    private PeDbContext pedb;

    private BillGenerateContext context;

    public BillGenerateBatchPdfService(IDbContextFactory<SwDbContext> swdbFactory, IDbContextFactory<PeDbContext> pedbFactory, IReportingService reportingService, IOptions<ReportingServiceOptions> options)
    {
        this.swdbFactory = swdbFactory;
        this.pedbFactory = pedbFactory;

        this._reportingService = reportingService;
        _options = options.Value;
    }

    public async Task Handle(BillGenerateContext context)
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
        context.Mthly_Bill_Generate_Beg_DateTime = await swdb.Get_Last_Bill_Tran_DateTime_For_Prev_Billing(context.Process_Date);
        context.Mthly_Bill_Generate_End_DateTime = await swdb.Get_Last_Bill_Tran_DateTime_For_Curr_Billing(context.Process_Date);

        //call the report server api to generate batch pdf sw bills file for email attachment.
        var parameters = new Dictionary<string, object>
                {
                    {"beg_datetime", context.Mthly_Bill_Generate_Beg_DateTime.ToString("MM/dd/yyyy HH:mm:ss.FFF")},
                    {"end_datetime", context.Mthly_Bill_Generate_End_DateTime.ToString("MM/dd/yyyy HH:mm:ss.FFF")},
                    {"customer_id", null},
                    //{"connectionString", _options.ConnectionString}
                };
        var byte_array = await _reportingService.GenerateReportPDF("SW_Bill", parameters);

        //return File(report, "application/pdf", "sw_bills_" + DateTime.Now + ".pdf");
        //System.IO.File.WriteAllBytes($"C:\\temp\\{reportName}.{"pdf"}", byte_array);

        context.BillGenerateBatchPdfWriter.Write(byte_array);
    }
}
