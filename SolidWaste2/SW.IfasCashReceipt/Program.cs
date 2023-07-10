using Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using SW.DAL.Extensions;
using SW.IfasCashReceipt.Services;

var cash_receipt_begin_datetime = DateTime.Now;
var cash_receipt_for_date = DateTime.Today.AddDays(-1);

CashReceiptContext context = new()
{
    CashReceiptBeginDatetime = cash_receipt_begin_datetime,
    CashReceiptForDate = cash_receipt_for_date,
};

var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", false).GetCurrentClassLogger();
//LogManager.Setup().LoadConfigurationFromSection(configuration, "NLog")
//LogManager.Setup().LoadConfigurationFromSection(configuration.GetSection("NLog"))

CashReceiptEmailService emailer = null;
try
{
    var hostBuilder = Host.CreateDefaultBuilder(args);

    hostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
    {
        var environment =
            args.Length == 1 ? args[0] :
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
            context.HostingEnvironment.EnvironmentName; // defaults to "Production"

        configBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", true);

        // key vault?
    });

    //hostBuilder.ConfigureLogging(logBuilder => {})
    //hostBuilder.config.ConfigureStackifyLogging()
    //StackifyLib.Config.SetConfiguration(configuration)
    //StackifyLib.Config.ReadStackifyJSONConfig()


    hostBuilder.ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddSingleton<IConfiguration>(configuration);

        services
            .AddSendGridService()
            .AddSolidWasteDbContext(configuration, "SolidWaste")
            .Configure<SendEmailSettings>(configuration.GetSection("Email"))
            .AddTransient<BillContainerDetailRepository>()
            .AddTransient<ContainerRateRepository>()
            .AddTransient<TransactionRepository>()
            .AddTransient<CashReceiptEmailService>()
            .AddTransient<CashReceiptReportService>()
            .AddTransient<CashReceiptUpdateService>();
    });

    var app = hostBuilder.Build();

    emailer = app.Services.GetRequiredService<CashReceiptEmailService>();

    var updater = app.Services.GetRequiredService<CashReceiptUpdateService>();
    await updater.Handle(context);

    var reporter = app.Services.GetRequiredService<CashReceiptReportService>();
    await reporter.Handle(context);

    await emailer.SendEmail(context);
}
catch (Exception e)
{
    for (var ex = e; ex != null; ex = ex.InnerException)
    {
        context.ExceptionWriter.WriteLine(ex.Message);
        context.ExceptionWriter.WriteLine(ex.StackTrace);
    }
    await emailer?.SendEmail(context);

    logger.Error(e, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();

    context.DeleteFiles();
}
