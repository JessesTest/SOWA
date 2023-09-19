using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SW.InternalWeb.Models.Reports;
using SW.BLL.Services;
using Common.Web.Extensions.Alerts;
using Microsoft.Extensions.Options;
using Common.Services.TelerikReporting;
using Microsoft.Graph;
using System.Data;
using SW.DM;

namespace SW.InternalWeb.Controllers
{    
    public class ReportsController : Controller
    {
        private readonly IReportingService _reportingService;
        private readonly ReportingServiceOptions _options;
        private readonly IPaymentPlanService _paymentPlanService;
        private readonly IConfiguration _configuration;

        public ReportsController(IReportingService reportingService, IOptions<ReportingServiceOptions> options, IPaymentPlanService paymentPlanService, IConfiguration configuration)
        {
            this._reportingService = reportingService;
            _options = options.Value;
            _paymentPlanService = paymentPlanService;
            _configuration = configuration;
        }

        [HttpPost]
        public ActionResult GetConnectionString() 
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get the report server and folder configuration
            var reportServerUrl = _configuration[$"{env}-Report-Server"];

            return Json(new
            {
                connection_string = _options.ConnectionString,
                report_server_url = reportServerUrl,
            });
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TransactionsbyDateRange(DateTime startDate, DateTime endDate, int customerId)
        {
            try 
            {
                var parameters = new Dictionary<string, object>
                {
                    {"startDate", startDate.ToString()},
                    {"endDate", endDate.ToString()},
                    {"customerID", customerId.ToString()},
                    //{"connectionString", _options.ConnectionString}
                };

                var report = await _reportingService.GenerateReportPDF("TransactionHistoryByCustomer", parameters);

                return File(report, "application/pdf", "transaction_history_" + customerId.ToString() + "_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "CustomerTransactionHistory", new { customerId = customerId }).WithDanger("", ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentPlanAgreement(int paymentPlanId)
        {
            try 
            {
                var parameters = new Dictionary<string, object>
                {
                    {"paymentPlanId", paymentPlanId},
                    //{"connectionString", _options.ConnectionString}
                };

                var report = await _reportingService.GenerateReportPDF("PaymentPlanAgreement", parameters);                

                return File(report, "application/pdf", "payment_plan_agreement_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                var payment_plan = await _paymentPlanService.GetById(paymentPlanId);

                return RedirectToAction("Index", "PaymentPlan", new { customerId = payment_plan.CustomerId }).WithDanger("", ex.Message);
            }
        }
    }
}
