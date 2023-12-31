using Common.Extensions;
using Identity.BL.Extensions;
using Identity.DAL.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Notify.BL.Extensions;
using Notify.DAL.Extensions;
using PE.BL.Extensions;
using PE.DAL.Extensions;
using StackifyLib;
using SW.BLL.Extensions;
using SW.DAL.Extensions;
using SW.ExternalWeb.Identity;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    configuration.AddEnvironmentVariables();
    // add key vault...

    // Add services to the container.
    builder.Services
        .AddRazorPages()
        .AddRazorRuntimeCompilation();

    builder.Services
        .AddControllersWithViews(mvcOptions => 
        {
            mvcOptions.EnableEndpointRouting = false;
        });

    // our services
    builder.Services
        .AddCommonServices(configuration)
        .AddPersonEntityDbContext(configuration)
        .AddPersonEntityServices()
        .AddIdentityDbContext(configuration)
        .AddIdentityServices()
        .AddNotifyDbContext(configuration)
        .AddNotifyServices(configuration)
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

    // identity
    builder.Services
        .AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Identity"))
        );
    builder.Services
        //.AddIdentity(configuration.GetConnectionString("Identity"))
        .AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // logging
    if (environment.IsEnvironment("Local"))
    {
        builder.Services
            .AddDatabaseDeveloperPageExceptionFilter();
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
    app.UseSession();

    app.UseSession();   // after UseRouting() and before Map...()

    app.MapRazorPages();
    app.MapDefaultControllerRoute();

    app.Run();
}
catch (Exception e)
{
    logger.Error(e, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
