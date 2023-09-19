using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace Common.Services.Common;

public class ApiServiceBase
{
    private readonly IConfidentialClientApplication _app;
    private readonly string _baseAddress;
    private readonly string _scope;
    private readonly ILogger _logger;

    public ApiServiceBase(
        string baseAddress,
        string scope,
        AuthSettings authSettings,
        ILogger logger)
    {
        _ = authSettings ?? throw new ArgumentNullException(nameof(authSettings));

        _baseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _app = ConfidentialClientApplicationBuilder.Create(authSettings.ClientId)
            .WithClientSecret(authSettings.ClientSecret)
            .WithAuthority(authSettings.Instance, authSettings.TenantId)
            .Build();
    }

    protected async Task<HttpResult> MakeRequest(ApiRequest api, object jsonContent)
    {
        var temp = await MakeRequest<object>(api, jsonContent);
        return new HttpResult
        {
            Message = temp.Message,
            ReasonPhrase = temp.ReasonPhrase,
            StatusCode = temp.StatusCode,
            Successful = temp.Successful
        };
    }

    protected async Task<HttpResult<T>> MakeRequest<T>(ApiRequest api, object jsonContent)
        where T : new()
    {
        _ = api ?? throw new ArgumentNullException(nameof(api));

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseAddress}{api.Path}")
            .PrepareRequest(_app, _scope)
            .SetJsonContent(jsonContent);

        var httpClient = new HttpClient();

        var response = await httpClient.SendAsync(request);

        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            _logger.LogDebug(JsonSerializer.Serialize(new
            {
                response.RequestMessage,
                response
            }));

        var responseStr = await response.Content.ReadAsStringAsync();

        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            _logger.LogDebug(responseStr);

        var jsonOptions = new JsonSerializerOptions();
        //jsonOptions.Converters.Add(new DecimalToJsonConverter())
        //jsonOptions.Converters.Add(new IntToJsonConverter())
        jsonOptions.PropertyNameCaseInsensitive = true;

        if (!response.IsSuccessStatusCode)
            return new HttpResult<T>
            {
                Successful = false,
                Value = default,
                Message = JsonSerializer.Deserialize<ErrorResult>(responseStr, jsonOptions).Title,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode
            };

        if (string.IsNullOrWhiteSpace(responseStr))
            return new HttpResult<T>
            {
                Successful = true,
                Value = default,
                Message = null,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode
            };
            
        return new HttpResult<T>
        {
            Successful = true,
            Value = JsonSerializer.Deserialize<T>(responseStr, jsonOptions),
            Message = null,
            ReasonPhrase = response.ReasonPhrase,
            StatusCode = response.StatusCode
        };
    }

    private sealed class ErrorResult
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
    }
}
