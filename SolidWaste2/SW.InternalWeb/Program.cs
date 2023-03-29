using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Common.Extensions;
using Identity.BL.Extensions;
using Identity.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using Microsoft.Identity.Web.UI;
using NLog;
using NLog.Web;
using PE.BL.Extensions;
using PE.DAL.Extensions;
using StackifyLib;
using SW.BLL.Extensions;
using SW.DAL.Extensions;
using SW.InternalWeb.Identity;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;
    configuration.AddEnvironmentVariables();    // for sendgrid

    //// key vault
    //var keyVaultEndpoint = new Uri(configuration["AzureKeyVaultEndpoint"]);
    //TokenCredential tokenCredential =
    //    builder.Environment.IsEnvironment("Local")
    //    ? new VisualStudioCredential()
    //    : new ManagedIdentityCredential();
    //configuration.AddAzureKeyVault(keyVaultEndpoint, tokenCredential, new AzureKeyVaultConfigurationOptions
    //{
    //    // Manager = new PrefixKeyVaultSecretManager(secretPrefix),
    //    ReloadInterval = TimeSpan.FromMinutes(5)
    //});

    // razor pages
    builder.Services
        .AddRazorPages();

    // mvc
    builder.Services
        .AddControllersWithViews(mvcOptions =>
        {
            mvcOptions.EnableEndpointRouting = false;
        })
        .AddRazorRuntimeCompilation();
        //.AddSessionStateTempDataProvider()
        //.AddMicrosoftIdentityUI()     // azure ad

    // our services
    builder.Services
        .AddCommonServices(configuration)
        .AddPersonEntityDbContext(configuration)
        .AddPersonEntityServices()
        .AddIdentityDbContext(configuration)
        .AddIdentityServices()
        .AddSolidWasteDbContext(configuration)
        .AddSolidWasteServices(configuration);

    // session state
    builder.Services
        .AddDistributedMemoryCache()
        .AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(12);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

    // cache security tokens
    builder.Services
        .Configure<MsalDistributedTokenCacheAdapterOptions>(options =>
        {
            options.DisableL1Cache = false;
            options.Encrypt = false;
            options.SlidingExpiration = TimeSpan.FromHours(1);
        })
        .AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("TokenCache");
            options.SchemaName = "dbo";
            options.TableName = "AADTokenCache";
        })
        .AddDistributedMemoryCache();

    //// azure ad, also uncomment .AddMicrosoftIdentityUI()
    //builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    //    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    // manage identity logins
    builder.Services.AddIdentity(configuration.GetConnectionString("Identity"));

    // logging
    if (environment.IsEnvironment("Local"))
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    }
    else
    {
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        builder.Host.UseNLog();
    }

    // cookie policy
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => false;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

    


    var app = builder.Build();

    if (app.Environment.IsEnvironment("Local"))
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.ConfigureStackifyLogging(configuration);
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSession();   // after UseRouting() and before Map...()

    app.MapRazorPages();
    app.MapDefaultControllerRoute();

    app.Run();
}
catch(Exception e)
{
    logger.Error(e, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}