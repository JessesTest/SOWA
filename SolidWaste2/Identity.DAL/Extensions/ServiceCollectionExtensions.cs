using Identity.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.DAL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityDbContext(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName = "Identity")

            => services.AddDbContextFactory<IdentityDbContext>(options => 
                options.UseSqlServer(configuration.GetConnectionString(connectionStringName)
                ));
    }
}
