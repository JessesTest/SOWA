using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SW.DAL.Contexts;

namespace SW.DAL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSolidWasteDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "SolidWaste")

        => services.AddDbContextFactory<SwDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString(connectionStringName)
            ));
}
