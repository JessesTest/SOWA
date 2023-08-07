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
using SW.Billing.Services;

DateTime process_date = DateTime.Now;

DateTime mthly_bill_beg_datetime = Convert.ToDateTime((process_date.AddMonths(-1).Year.ToString().PadLeft(4, '0') + "-" + process_date.AddMonths(-1).Month.ToString().PadLeft(2, '0') + "-" + "01" + " 12:00:00 AM"));
DateTime mthly_bill_end_datetime = Convert.ToDateTime((process_date.AddMonths(-1).Year.ToString().PadLeft(4, '0') + "-" + process_date.AddMonths(-1).Month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(Convert.ToInt32(process_date.AddMonths(-1).Year), Convert.ToInt32(process_date.AddMonths(-1).Month)).ToString().PadLeft(2, '0') + " 11:59:59 PM"));

var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", false).GetCurrentClassLogger();

BillingEmailService billing_email_service = null;

BillingContext context = new()
{
    Process_Date = process_date,

    Mthly_Bill_Beg_DateTime = mthly_bill_beg_datetime,
    Mthly_Bill_End_DateTime = mthly_bill_end_datetime
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
            .AddTransient<BillingEmailService>()
            .AddTransient<BillingUpdateService>()
            .AddAzureServices(configuration)
            .AddBlobStorageService(configuration, "BlobService")
            .AddTransient<BillingBlobService>(); 
    });

    var app = hostBuilder.Build();

    billing_email_service = app.Services.GetRequiredService<BillingEmailService>();

    var billing_update_service = app.Services.GetRequiredService<BillingUpdateService>();
    await billing_update_service.Handle(context);

    //var billing_blob_service = app.Services.GetRequiredService<BillingBlobService>();
    //await billing_blob_service.Handle(context);

    await billing_email_service.SendEmail(context);
}
catch (Exception e)
{
    for (var ex = e; ex != null; ex = ex.InnerException)
    {
        context.BillingExceptionWriter.WriteLine(ex.Message);
        context.BillingExceptionWriter.WriteLine(ex.StackTrace);
    }

    await billing_email_service?.SendEmail(context);

    logger.Error(e, "Exception encountered processing SW Bills");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
    context.DeleteFiles();
}