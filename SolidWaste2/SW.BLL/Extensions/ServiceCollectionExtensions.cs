using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SW.BLL.Services;

namespace SW.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSolidWasteServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<IBillBlobService, BillBlobService>()
            .AddTransient<IBillContainerService, BillContainerService>()
            .AddTransient<IBillMasterService, BillMasterService>()
            .AddTransient<IBillingSummaryService, BillingSummaryService>()
            .AddTransient<IContainerCodeService, ContainerCodeService>()
            .AddTransient<IContainerRateService, ContainerRateService>()
            .AddTransient<IContainerService, ContainerService>()
            .AddTransient<IContainerSubtypeService, ContainerSubtypeService>()
            .AddTransient<ICustomerService, CustomerService>()
            .AddTransient<IEquipmentLegacyService, EquipmentLegacyService>()
            .AddTransient<IKanPayService, KanPayService>()
            .AddTransient<IMieDataService, MieDataService>()
            .AddTransient<IPaymentPlanService, PaymentPlanService>()
            .AddTransient<IRefuseRouteService, RefuseRouteService>()
            .AddTransient<IServiceAddressService, ServiceAddressService>()
            .AddTransient<ITransactionCodeService, TransactionCodeService>()
            .AddTransient<ITransactionService, TransactionService>()
            .AddTransient<IWorkOrderService, WorkOrderService>()
            .AddTransient<IWorkOrderLegacyService, WorkOrderLegacyService>();

        services
            .Configure<KanPaySettings>(configuration.GetSection("KanPay"));

        services
            .AddHttpClient("PickupRoutes", client =>
            {
                client.BaseAddress = new Uri(configuration["Routes:Url"]);
            });

        return services;
    }
}
