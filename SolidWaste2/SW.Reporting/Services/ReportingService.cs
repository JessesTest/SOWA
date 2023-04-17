using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.Reporting.OpenXmlRendering;

namespace SW.Reporting.Services
{
    public interface IReportingService
    {
        Task<byte[]> GenerateReportPDF(string reportName);
        Task<byte[]> GenerateReportPDF(string reportName, IDictionary<string, object> parameters);
        Task<byte[]> GenerateReportXLS(string reportName);
        Task<byte[]> GenerateReportXLS(string reportName, IDictionary<string, object> parameters);
    }

    public class ReportingService : IReportingService
    {
        private readonly ReportingServiceOptions _options;

        public ReportingService(IOptions<ReportingServiceOptions> options)
        {
            _options = options.Value;
        }

        public Task<byte[]> GenerateReportPDF(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"{nameof(reportName)} cannot be null or whitespace.");

            return GenerateReportPDFInternalAsync(reportName, parameters: null);
        }

        public Task<byte[]> GenerateReportPDF(string reportName, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"{nameof(reportName)} cannot be null or whitespace.");

            return GenerateReportPDFInternalAsync(reportName, parameters);
        }

        private async Task<byte[]> GenerateReportPDFInternalAsync(string reportName, IDictionary<string, object>? parameters)
        {
            var reportProcessor = new ReportProcessor();
            var deviceInfo = new System.Collections.Hashtable();

            var reportSource = new UriReportSource();
            reportSource.Parameters.Add("ConnectionString", _options.ConnectionString);
            reportSource.Uri = $"Reports/{reportName}.trdp";

            if (parameters != null)
                foreach (var p in parameters)
                    reportSource.Parameters.Add(p.Key, p.Value);

            var result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

            if (result.HasErrors)
                throw result.Errors.First();

            return result.DocumentBytes;
        }

        public Task<byte[]> GenerateReportXLS(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"{nameof(reportName)} cannot be null or whitespace.");

            return GenerateReportXLSInternalAsync(reportName, parameters: null);
        }

        public Task<byte[]> GenerateReportXLS(string reportName, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"{nameof(reportName)} cannot be null or whitespace.");

            return GenerateReportXLSInternalAsync(reportName, parameters);
        }

        private async Task<byte[]> GenerateReportXLSInternalAsync(string reportName, IDictionary<string, object>? parameters)
        {
            var reportProcessor = new ReportProcessor();
            var deviceInfo = new System.Collections.Hashtable();

            var reportSource = new UriReportSource();
            reportSource.Parameters.Add("ConnectionString", _options.ConnectionString);
            reportSource.Uri = $"Reports/{reportName}.trdp";

            if (parameters != null)
                foreach (var p in parameters)
                    reportSource.Parameters.Add(p.Key, p.Value);

            var result = reportProcessor.RenderReport("XLSX", reportSource, deviceInfo);

            if (result.HasErrors)
                throw result.Errors.First();

            return result.DocumentBytes;
        }
    }

    public class ReportingServiceOptions
    {
        public string ConnectionString { get; set; }
    }
}
