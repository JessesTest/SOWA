using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notify.BL.Services;

namespace Notify.BL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyServices(this IServiceCollection services, IConfiguration configuration) => services
        .Configure<NotifySettings>(configuration.GetSection("Notify"))
        .AddTransient<INotifyService, NotifyService>();
}
