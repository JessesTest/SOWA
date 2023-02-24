using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PE.DAL.Contexts;

namespace PE.DAL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersonEntityDbContext(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName = "PersonEntity")

            => services.AddDbContextFactory<PeDbContext>(options => options.UseSqlServer(
                    configuration.GetConnectionString(connectionStringName),
                    opt => opt.UseNetTopologySuite()    // for geography
                ));
    }
}
