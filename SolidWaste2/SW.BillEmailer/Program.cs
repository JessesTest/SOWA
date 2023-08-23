using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using SW.DAL.Extensions;
using PE.DAL.Extensions;
using SW.BillEmailer.Services;
using System.Globalization;
using Common.Services.TelerikReporting;

var process_date_beg = DateTime.Today.AddMonths(-1).AddDays(1 - DateTime.Today.Day);
var process_date_end = DateTime.Today.AddMonths(1).AddDays(-1);

var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", false).GetCurrentClassLogger();

BillEmailerEmailService bill_emailer_email_service = null;

BillEmailerContext context = new()
{
    Process_Date_Beg = process_date_beg,
    Process_Date_End = process_date_end
};

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

        var settings = configBuilder.Build();
        var keyVaultEndpoint = new Uri(settings["KeyVault:Endpoint"]);
        TokenCredential tokenCredential = environment.ToUpper() == "LOCAL" ?
            new VisualStudioCredential() :
            new ManagedIdentityCredential();
        var keyVaultOptions = new AzureKeyVaultConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromMinutes(5)
        };
        configBuilder
            .AddAzureKeyVault(keyVaultEndpoint, tokenCredential, keyVaultOptions);

    });

    hostBuilder.ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddSingleton<IConfiguration>(configuration);

        services
            .AddSendGridService()
            .AddSolidWasteDbContext(configuration, "SolidWaste")
            .AddPersonEntityDbContext(configuration, "PersonEntity")
            .Configure<SendEmailSettings>(configuration.GetSection("Email"))
            .AddTransient<BillEmailerEmailService>()
            .AddTransient<BillEmailerGenerateService>()
            .AddAzureServices(configuration)
            .AddBlobStorageService(configuration, "BlobService")
            .AddTransient<BillEmailerStorageService>()
            .AddTransient<IReportingService, ReportingService>()
            .AddOptions<ReportingServiceOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("SolidWaste");
            });
    });

    var app = hostBuilder.Build();

    bill_emailer_email_service = app.Services.GetRequiredService<BillEmailerEmailService>();

    var bill_emailer_generate_service = app.Services.GetRequiredService<BillEmailerGenerateService>();
    await bill_emailer_generate_service.Handle(context);

    //var bill_emailer_storage_service = app.Services.GetRequiredService<BillEmailerStorageService>();
    //await bill_emailer_storage_service.Handle(context);

    await bill_emailer_email_service.SendEmail(context);
}
catch (Exception e)
{
    for (var ex = e; ex != null; ex = ex.InnerException)
    {
        context.BillEmailerExceptionWriter.WriteLine(ex.Message);
        context.BillEmailerExceptionWriter.WriteLine(ex.StackTrace);
    }

    await bill_emailer_email_service?.SendEmail(context);

    logger.Error(e, "Exception encountered generating SW Bill emails!");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
    context.DeleteFiles();
}