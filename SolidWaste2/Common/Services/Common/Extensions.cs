using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Common.Web.Services.Common;

public static class Extensions
{
    public static HttpRequestMessage SetJsonContent(this HttpRequestMessage request, object jsonContent)
    {
        if (jsonContent != null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestStr = JsonSerializer.Serialize(jsonContent);
            var content = new StringContent(requestStr, Encoding.UTF8, MediaTypeNames.Application.Json);
            request.Content = content;
        }

        return request;
    }

    public static HttpRequestMessage PrepareRequest(this HttpRequestMessage request, ITokenAcquisition tokenAcquisition, string scope)
    {
        var accessToken = Task.Run(() => tokenAcquisition.GetAccessTokenForAppAsync(scope, tokenAcquisitionOptions: new TokenAcquisitionOptions
        {
            ForceRefresh = true
        })).Result;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        return request;
    }

    public static HttpRequestMessage PrepareRequest(this HttpRequestMessage request, IConfidentialClientApplication app, string scope)
    {
        var accessToken = Task.Run(() => app.AcquireTokenForClient(new[] { scope })
            .WithForceRefresh(true)
            .ExecuteAsync()).Result.AccessToken;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        return request;
    }
}
