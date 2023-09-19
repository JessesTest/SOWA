using Microsoft.Extensions.DependencyInjection;
using PE.BL.Services;

namespace PE.BL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersonEntityServices(
            this IServiceCollection services)
            => services
            .AddAddressService()
            .AddCodeService()
            .AddEmailService()
            .AddPersonEntityService()
            .AddPhoneService();

        public static IServiceCollection AddAddressService(this IServiceCollection services)
            => services.AddTransient<IAddressService, AddressService>();

        public static IServiceCollection AddCodeService(this IServiceCollection services)
            => services.AddTransient<ICodeService, CodeService>();

        public static IServiceCollection AddEmailService(this IServiceCollection services)
            => services.AddTransient<IEmailService, EmailService>();

        public static IServiceCollection AddPersonEntityService(this IServiceCollection services)
            => services.AddTransient<IPersonEntityService, PersonEntityService>();

        public static IServiceCollection AddPhoneService(this IServiceCollection services)
            => services.AddTransient<IPhoneService, PhoneService>();
    }
}
