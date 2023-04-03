using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace SW.BLL.Services;

public class RefuseRouteService : IRefuseRouteService
{
    private readonly IHttpClientFactory httpClientFactory;

    public RefuseRouteService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<RouteResponse> SearchRefuseRoute(
        string address,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> data = new()
        {
            { "SingleKey", address },
            { "outFields", "User_fld" },
            { "f", "json" }
        };

        return await GetRequest<RouteResponse>(data, cancellationToken);
    }

    internal async Task<T> GetRequest<T>(IDictionary<string, object> data, CancellationToken cancellationToken)
        where T : new()
    {
        using var client = httpClientFactory.CreateClient("PickupRoutes");

        StringBuilder builder = new(256);
        foreach (var pair in data)
        {
            builder
                .Append(builder.Length == 0 ? '?' : '&')
                .Append(WebUtility.UrlEncode(pair.Key))
                .Append('=')
                .Append(WebUtility.UrlEncode(pair.Value.ToString()));
        }

        return await client.GetFromJsonAsync<T>(builder.ToString(), cancellationToken);
    }
}
