using Common.Extensions;
using Identity.BL.Extensions;
using Identity.DAL.Extensions;
using NLog;
using NLog.Web;
using PE.BL.Extensions;
using PE.DAL.Extensions;
using StackifyLib;
using SW.BLL.Extensions;
using SW.DAL.Extensions;
using System;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    configuration.AddEnvironmentVariables();
    // add key valut...

    // Add services to the container.
    builder.Services
        .AddRazorPages();

    builder.Services
        .AddControllersWithViews(mvcOptions => 
        {
            mvcOptions.EnableEndpointRouting = false;
        });

    builder.Services
        .AddCommonServices(configuration)
        .AddPersonEntityDbContext(configuration)
        .AddPersonEntityServices()
        .AddIdentityDbContext(configuration)
        .AddIdentityServices()
        .AddSolidWasteDbContext(configuration)
        .AddSolidWasteServices();


    if (!environment.IsEnvironment("Local"))
    {
        // ...
    }
    else
    {
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        builder.Host.UseNLog();
    }

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

    //app.UseAuthentication()
    app.UseAuthorization();

    //app.UseSession()

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



