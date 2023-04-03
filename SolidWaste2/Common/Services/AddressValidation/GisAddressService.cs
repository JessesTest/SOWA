using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Common.Services.AddressValidation;

public class GisAddressService : IAddressValidationService
{
    private readonly IHttpClientFactory httpClientFactory;

    public GisAddressService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<IList<ValidAddress>> GetCandidates(
        string address,
        string city = null,
        string zip5 = null,
        int maxLocations = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("An address is required", nameof(address));

        var parameters = new Dictionary<string, object>
        {
            { "address", address }, // Address or SingleLine
            { "f", "json" },
            { "maxLocations", maxLocations }
        };

        if (!string.IsNullOrWhiteSpace(city))
            parameters.Add("city", city);

        if (!string.IsNullOrWhiteSpace(zip5))
            parameters.Add("postal", zip5);

        var candidatesResult = await GetRequest<GisResult>(parameters, cancellationToken);

        return candidatesResult.Candidates
             .Select(c => new ValidAddress(c.Address, c.Score))
             .ToList();
    }

    internal async Task<T> GetRequest<T>(IDictionary<string, object> data, CancellationToken cancellationToken)
        where T : new()
    {
        using var client = httpClientFactory.CreateClient("GisAddress");

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
