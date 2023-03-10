using Microsoft.Extensions.DependencyInjection;
using SW.BLL.Services;

namespace SW.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSolidWasteServices(this IServiceCollection services)
        => services
            .AddTransient<ICustomerService, CustomerService>();

}
