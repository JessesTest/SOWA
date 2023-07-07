using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Common.Services.TelerikReporting
{
    public interface IReportingService
    {
        Task<byte[]> GenerateReportPDF(string reportName, IDictionary<string, object> parameters);
        Task<byte[]> GenerateReportXLS(string reportName, IDictionary<string, object> parameters);
    }

    public class ReportingService : IReportingService
    {
        //export report from telerik report server via .net client

        private readonly IConfiguration _configuration;

        public ReportingService(IConfiguration configuration, IOptions<ReportingServiceOptions> options)
        {
            _configuration = configuration;
        }

        public Task<byte[]> GenerateReportPDF(string reportName, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"{nameof(reportName)} cannot be null or whitespace.");

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];
            var reportServerApiUrl = reportServerUrl + _configuration[$"{env}-Api-Report-Server-Guest"];

            //return GenerateReportPDFInternalAsync(reportName, parameters);
            return GetReportFromTelerikReportServiceAsync("SolidWaste/" + reportName + ".trdp", "PDF", parameters, reportServerApiUrl);
        }
        
        private async Task<byte[]> GenerateReportPDFInternalAsync(string reportName, IDictionary<string, object> parameters)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];
            var reportServerApiUrl = reportServerUrl + _configuration[$"{env}-Api-Report-Server-Admin"];
            var username = _configuration[$"{env}-Report-Username"];
            var password = _configuration[$"{env}-Report-Password"];

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(reportServerApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Authenticate via OAuth 2.0 Password Grant.
                var authToken = GetAuthenticationToken(client, username, password, reportServerUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                // List all reports.
                var reports = await GetAllReports(client, reportServerApiUrl);

                // Render the desired report.
                var report = reports.Find(r => r["Name"].Equals(reportName))["Id"];
                var documentUrl = await CreateDocument(client, reportServerApiUrl, "PDF", report, parameters);

                // Download the created document.
                var downloadPath = DownloadDocument(documentUrl, reportName, ".pdf", false);
                var byte_array = System.IO.File.ReadAllBytes(downloadPath);
                return byte_array;
            }
        }

        public Task<byte[]> GenerateReportXLS(string reportName, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"{nameof(reportName)} cannot be null or whitespace.");

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];
            var reportServerApiUrl = reportServerUrl + _configuration[$"{env}-Api-Report-Server-Guest"];

            //return GenerateReportXLSInternalAsync(reportName, parameters);
            return GetReportFromTelerikReportServiceAsync("SolidWaste/" + reportName + ".trdp", "XLSX", parameters, reportServerApiUrl);
        }

        private async Task<byte[]> GenerateReportXLSInternalAsync(string reportName, IDictionary<string, object> parameters)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];
            var reportServerApiUrl = reportServerUrl + _configuration[$"{env}-Api-Report-Server-Admin"];
            var username = _configuration[$"{env}-Report-Username"];
            var password = _configuration[$"{env}-Report-Password"];

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(reportServerApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Authenticate via OAuth 2.0 Password Grant.
                var authToken = GetAuthenticationToken(client, username, password, reportServerUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                // List all reports.
                var reports = await GetAllReports(client, reportServerApiUrl);

                // Render the desired report.
                var report = reports.Find(r => r["Name"].Equals(reportName))["Id"];
                var documentUrl = await CreateDocument(client, reportServerApiUrl, "XLSX", report, parameters);

                // Download the created document.
                var downloadPath = DownloadDocument(documentUrl, reportName, ".xlsx", false);
                var byte_array = System.IO.File.ReadAllBytes(downloadPath);
                return byte_array;
            }
        }

        static string GetAuthenticationToken(HttpClient client, string usernameInput, string passwordInput, string reportServerUrl)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", usernameInput),
                new KeyValuePair<string, string>("password", passwordInput)
            });

            // POST Token
            var response = client.PostAsync(reportServerUrl + "Token", data).Result;
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            var token = JObject.Parse(result).SelectToken("access_token").ToString();

            return token;
        }

        static async Task<List<Dictionary<string, string>>> GetAllReports(HttpClient client, string apiUrl)
        {
            // GET api/reportserver/reports
            var response = await client.GetAsync(apiUrl + "reports");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<List<Dictionary<string, string>>>();
            }

            return null;
        }

        static async Task<string> CreateDocument(HttpClient client, string apiUrl, string format, string reportId, object parameterValues)
        {
            var data = new { ReportId = reportId, Format = format, ParameterValues = parameterValues };
            // POST api/reportserver/documents
            var response = await client.PostAsJsonAsync(apiUrl + "documents", data);
            response.EnsureSuccessStatusCode();

            var documentUrl = response.Headers.Location;

            return documentUrl.ToString();
        }

        static string DownloadDocument(string url, string reportName, string extension, bool asAttachment)
        {
            var queryString = asAttachment ? "?content-disposition=attachment" : "";
            var fileName = reportName + extension;
            var folderName = System.IO.Path.GetTempPath();
            var filePath = System.IO.Path.Combine(folderName, fileName);

            using (var webClient = new System.Net.WebClient())
            {
                webClient.DownloadFile(url + queryString, filePath);
            }

            return filePath;
        }
        
        private async Task<byte[]> GetReportFromTelerikReportServiceAsync(string reportName, string format, IDictionary<string, object> parameters, string baseUrl)
        {
            // 1. Register client
            ReportClient reportClient = new ReportClient(baseUrl);
            await reportClient.RegisterClient();


            // 2. Create Report Instance
            ReportSourceModel reportSourceModel = new ReportSourceModel()
            {
                Report = reportName,
                ParameterValues = parameters
            };

            string reportSource = System.Text.Json.JsonSerializer.Serialize(reportSourceModel);
            string reportInstanceId = await reportClient.CreateInstance(reportSource);

            // 3. Create Document
            string reportDocumentId = await reportClient.CreateDocument(reportInstanceId, format);

            bool documentProcessing;
            do
            {
                Thread.Sleep(500);// wait before next Info request
                documentProcessing = await reportClient.DocumentIsProcessing(reportInstanceId, reportDocumentId);
            } while (documentProcessing);

            // 4. Get Document
            byte[] byte_array = await reportClient.GetDocument(reportInstanceId, reportDocumentId);
            return byte_array;
            //File.WriteAllBytes($"C:\\temp\\{reportName}.{format.ToLower()}", byte_array); 
        }
    }

    public class ReportingServiceOptions
    {
        public string ConnectionString { get; set; }
    }
    
    public class ClientIDModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("clientId")]
        public string ClientId { get; set; }
    }

    public class ReportSourceModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("Report")]
        public string Report { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("ParameterValues")]
        public IDictionary<string, object> ParameterValues { get; set; }
    }

    public class InstanceIdModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("instanceId")]
        public string InstanceId { get; set; }
    }

    public class DocumentIdModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("documentId")]
        public string DocumentId { get; set; }
    }

    public class DocumentInfoModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("documentReady")]
        public bool DocumentReady { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("PageCount")]
        public int PageCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("DocumentMapAvailable")]
        public bool DocumentMapAvailable { get; set; }
    }

    public class ErrorModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public string Error { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error_description")]
        public string Description { get; set; }
    }

    //public static class HttpContentExtensions
    //{
    //    public static async Task<T> ReadAsAsync<T>(this HttpContent content) =>
    //        await System.Text.Json.JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync());
    //}

    public class ReportClient : IDisposable
    {
        public string BaseAddress { get; set; }
        public HttpClient client;
        public string ClientId;

        public ReportClient(string uri)
        {
            this.client = HttpClientFactory.Create();
            this.BaseAddress = uri;

            this.client.BaseAddress = new Uri(this.BaseAddress);
        }

        public void Dispose()
        {
            using (this.client) { }
        }

        public async Task RegisterClient()
        {
            var headers = new List<KeyValuePair<string, string>>();
            var content = new FormUrlEncodedContent(headers);

            var response = await this.client.PostAsync(this.BaseAddress + "/clients", content);

            if (response.IsSuccessStatusCode)
            {
                var clientIdTask = await response.Content.ReadAsAsync<ClientIDModel>();
                this.ClientId = clientIdTask.ClientId;
            }
            else
            {
                var error = await response.Content.ReadAsAsync<ErrorModel>();
                throw new Exception(error.Description);
            }
        }

        public async Task<string> CreateInstance(string reportSource)
        {
            HttpContent content = new StringContent(reportSource, System.Text.Encoding.UTF8, "application/json");

            string route = $"{this.BaseAddress}/clients/{this.ClientId}/instances";
            var response = await this.client.PostAsync(route, content);

            InstanceIdModel instanceId = null;
            if (response.IsSuccessStatusCode)
            {
                instanceId = await response.Content.ReadAsAsync<InstanceIdModel>();
            }
            else
            {
                var error = await response.Content.ReadAsAsync<ErrorModel>();
                throw new Exception(error.Description);
            }

            return instanceId.InstanceId;
        }

        public async Task<string> CreateDocument(string instanceId, string format, string deviceInfo = null, string useCache = "true")
        {
            string contentBody = $"{{ \"useCache\": {useCache}, \"format\": \"{format}\" }}";
            HttpContent content = new StringContent(contentBody, System.Text.Encoding.UTF8, "application/json");

            string route = $"{this.BaseAddress}/clients/{this.ClientId}/instances/{instanceId}/documents";
            var response = await this.client.PostAsync(route, content);

            DocumentIdModel documentId = null;
            if (response.IsSuccessStatusCode)
            {
                documentId = await response.Content.ReadAsAsync<DocumentIdModel>();
            }
            else
            {
                var error = await response.Content.ReadAsAsync<ErrorModel>();
                throw new Exception(error.Description);
            }

            return documentId.DocumentId;
        }

        public async Task<bool> DocumentIsProcessing(string instanceId, string documentId)
        {
            string route = $"{this.BaseAddress}/clients/{this.ClientId}/instances/{instanceId}/documents/{documentId}/Info";

            var response = await this.client.GetAsync(route);

            DocumentInfoModel documentInfo = null;
            if (response.IsSuccessStatusCode)
            {
                documentInfo = await response.Content.ReadAsAsync<DocumentInfoModel>();
            }
            else
            {
                var error = await response.Content.ReadAsAsync<ErrorModel>();
                throw new Exception(error.Description);
            }

            return !documentInfo.DocumentReady;
        }

        public async Task<byte[]> GetDocument(string instanceId, string documentId)
        {
            string route = $"{this.BaseAddress}/clients/{this.ClientId}/instances/{instanceId}/documents/{documentId}";

            var response = await this.client.GetAsync(route);
            byte[] documentBytes;

            if (response.IsSuccessStatusCode)
            {
                documentBytes = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsAsync<ErrorModel>();
                throw new Exception(error.Description);
            }

            return documentBytes;
        }

        //private static void EnsureSuccessStatusCode(HttpResponseMessage response)
        //{
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        throw
        //            new Exception(
        //                response.ReasonPhrase +
        //                Environment.NewLine +
        //                response.RequestMessage.RequestUri);
        //    }
        //}
    }
}
