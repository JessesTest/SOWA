using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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

            return GenerateReportPDFInternalAsync(reportName, parameters);
        }
        
        private async Task<byte[]> GenerateReportPDFInternalAsync(string reportName, IDictionary<string, object> parameters)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];
            var reportServerApiUrl = reportServerUrl + _configuration[$"{env}-Api-Report-Server"];
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

            return GenerateReportXLSInternalAsync(reportName, parameters);
        }

        private async Task<byte[]> GenerateReportXLSInternalAsync(string reportName, IDictionary<string, object> parameters)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];
            var reportServerApiUrl = reportServerUrl + _configuration[$"{env}-Api-Report-Server"];
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
    }

    public class ReportingServiceOptions
    {
        public string ConnectionString { get; set; }
    }
}
