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
using SW.BillGenerate.Services;
using System.Globalization;
using Common.Services.TelerikReporting;

var process_date = DateTime.Parse($"{DateTime.Today.Year}-{DateTime.Today.Month}-01", new CultureInfo("en-US"));

var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", false).GetCurrentClassLogger();

BillGenerateEmailService bill_generate_email_service = null;

BillGenerateContext context = new()
{
    Process_Date = process_date
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
            .AddTransient<BillGenerateEmailService>()
            .AddTransient<BillGenerateBatchPdfService>()
            .AddAzureServices(configuration)
            .AddBlobStorageService(configuration, "BlobService")
            .AddTransient<BillGenerateBlobService>()
            .AddTransient<IReportingService, ReportingService>()
            .AddOptions<ReportingServiceOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("SolidWaste");
            });
    });

    var app = hostBuilder.Build();

    bill_generate_email_service = app.Services.GetRequiredService<BillGenerateEmailService>();

    var bill_generate_batch_pdf_service = app.Services.GetRequiredService<BillGenerateBatchPdfService>();
    await bill_generate_batch_pdf_service.Handle(context);

    //var bill_generate_blob_service = app.Services.GetRequiredService<BillGenerateBlobService>();
    //await bill_generate_blob_service.Handle(context);

    await bill_generate_email_service.SendEmail(context);
}
catch (Exception e)
{
    for (var ex = e; ex != null; ex = ex.InnerException)
    {
        context.BillGenerateExceptionWriter.WriteLine(ex.Message);
        context.BillGenerateExceptionWriter.WriteLine(ex.StackTrace);
    }

    await bill_generate_email_service?.SendEmail(context);

    logger.Error(e, "Exception encountered generating SW Bills batch PDF!");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
    context.DeleteFiles();
}