using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SW.ExternalWeb.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentity(
            this IServiceCollection services,
            string connectionString)
        {
            services
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString))
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;  // ?
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
