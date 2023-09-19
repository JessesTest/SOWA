using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using SW.DAL.Extensions;
using SW.WeeklyRental.Services;

var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", false).GetCurrentClassLogger();
WeeklyRentalEmailService emailer = null;
WeeklyRentalContext context = new()
{
    CurrentDate = DateTime.Today,
    CurrentTime = DateTime.Now,
    BeginTime = DateTime.Now
    //EndTime = DateTime.Today.AddDays(6)
};
try
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, configBuilder) => {

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
                // Manager = new PrefixKeyVaultSecretManager(secretPrefix),
                ReloadInterval = TimeSpan.FromMinutes(5)
            };
            configBuilder
                .AddAzureKeyVault(keyVaultEndpoint, tokenCredential, keyVaultOptions);
        })
        //.ConfigureLogging(logBuilder => { })
        .ConfigureServices((context, services) => {
            var configuration = context.Configuration;
            services.AddSingleton<IConfiguration>(configuration);

            services
                .AddSendGridService()
                .AddSolidWasteDbContext(configuration, "SolidWaste")
                .Configure<SendEmailSettings>(configuration.GetSection("Email"))
                .AddTransient<WeeklyRentalEmailService>()
                .AddTransient<WeeklyRentalUpdateService>()
                .AddAzureServices(configuration)
                .AddBlobStorageService(configuration, "BlobService")
                .AddTransient<WeeklyRentalBlobService>();
        });

    var app = hostBuilder.Build();

    emailer = app.Services.GetRequiredService<WeeklyRentalEmailService>();

    var updater = app.Services.GetRequiredService<WeeklyRentalUpdateService>();
    await updater.Handle(context);

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
