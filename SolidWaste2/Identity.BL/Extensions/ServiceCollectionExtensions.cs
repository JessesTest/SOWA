using Identity.BL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.BL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
            => services
                .AddTransient<IUserNotificationService, UserNotificationService>()
                .AddTransient<IUserService, UserService>();
    }
}
