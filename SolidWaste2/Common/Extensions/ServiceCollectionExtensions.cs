using Common.Services.AddressValidation;
using Common.Services.Blob;
using Common.Services.Email;
using Common.Services.Sms;
using Common.Services.Common;
using Common.Services.GraphApi;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSendGridService()
            .AddTwilioService(configuration)
            .AddAzureServices(configuration)
            .AddBlobStorageService(configuration)
            .AddGraphService(configuration)
            .AddGisAddressService(configuration);
    }

    public static IServiceCollection AddSendGridService(this IServiceCollection services)
    {
        return services
            .AddTransient<ISendGridService, SendGridService>();
    }

    public static IServiceCollection AddTwilioService(this IServiceCollection services, IConfiguration configuration, string sectionName = "Twilio")
    {
        return services
            .Configure<TwilioSettings>(configuration.GetSection(sectionName))
            .AddTransient<ITwilioService, TwilioService>();
    }

    public static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration, string sectionName = "BlobService")
    {
        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(configuration["BlobStorage:ConnectionString"]);
        });

        return services;
    }

    public static IServiceCollection AddBlobStorageService(this IServiceCollection services, IConfiguration configuration, string sectionName = "BlobService")
        => services
            .Configure<BlobStorageServiceSettings>(configuration.GetSection(sectionName))
            .AddTransient<IBlobStorageService, BlobStorageService>();

    public static IServiceCollection AddGraphService(this IServiceCollection services, IConfiguration configuration, string graphSection = "Graph", string authSection = "AzureAd")
        => services
            .Configure<GraphServiceSettings>(configuration.GetSection(graphSection))
            .Configure<AuthSettings>(configuration.GetSection(authSection))
            .AddHttpContextAccessor()
            .AddTransient<IGraphService, GraphService>();

    public static IServiceCollection AddGisAddressService(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<IAddressValidationService, GisAddressService>()
            .AddHttpClient("GisAddress", client =>
            {
                client.BaseAddress = new Uri(configuration["GisAddress:Url"]);
            });

        return services;
    }
}
