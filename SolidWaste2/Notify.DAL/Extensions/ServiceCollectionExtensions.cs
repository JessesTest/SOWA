using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notify.DAL.Contexts;

namespace Notify.DAL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Notify")

        => services.AddDbContextFactory<NotifyDbContext>(options => 
        options.UseSqlServer( configuration.GetConnectionString(connectionStringName) ));
}
