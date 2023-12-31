﻿using SW.InternalWeb.Services;

namespace SW.InternalWeb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInternalWebServices(this IServiceCollection services)
        => services
        .AddTransient<BillingSizeSelectItemsService>()
        .AddTransient<ContainerCodeSelectItemsService>()
        .AddTransient<ContainerSubtypeSelectItemsService>()
        .AddTransient<EmailTypeSelectItemsService>()
        .AddTransient<PhoneTypeSelectItemsService>()
        .AddTransient<WriteoffCodeSelectItemService>();
}
