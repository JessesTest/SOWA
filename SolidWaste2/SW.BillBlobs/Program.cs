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
using SW.BillBlobs.Services;
using System.Globalization;
using Common.Services.TelerikReporting;

var process_date_beg = DateTime.Parse($"{DateTime.Today.Year}-{DateTime.Today.Month}-01", new CultureInfo("en-US"));
var process_date_end = DateTime.Now;

var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", false).GetCurrentClassLogger();

BillBlobEmailService bill_blob_email_service = null;

BillBlobContext context = new()
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
            .AddTransient<BillBlobEmailService>()
            .AddTransient<BillBlobGenerateService>()
            .AddAzureServices(configuration)
            .AddBlobStorageService(configuration, "BlobService")
            .AddTransient<BillBlobStorageService>()
            .AddTransient<IReportingService, ReportingService>()
            .AddOptions<ReportingServiceOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("SolidWaste");
            });
    });

    var app = hostBuilder.Build();

    bill_blob_email_service = app.Services.GetRequiredService<BillBlobEmailService>();

    var bill_blob_generate_service = app.Services.GetRequiredService<BillBlobGenerateService>();
    await bill_blob_generate_service.Handle(context);

    //var bill_blob_storage_service = app.Services.GetRequiredService<BillBlobStorageService>();
    //await bill_blob_storage_service.Handle(context);

    await bill_blob_email_service.SendEmail(context);
}
catch (Exception e)
{
    for (var ex = e; ex != null; ex = ex.InnerException)
    {
        context.BillBlobExceptionWriter.WriteLine(ex.Message);
        context.BillBlobExceptionWriter.WriteLine(ex.StackTrace);
    }

    await bill_blob_email_service?.SendEmail(context);

    logger.Error(e, "Exception encountered generating SW Bill blobs!");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
    context.DeleteFiles();
}