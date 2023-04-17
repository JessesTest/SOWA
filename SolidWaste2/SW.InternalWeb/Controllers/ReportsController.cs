using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SW.DM;
using SW.InternalWeb.Models.Reports;
using SW.BLL.Services;
using PE.BL.Services;
using SW.Reporting.Services;
using Common.Web.Extensions.Alerts;
using PE.DM;
using System.Collections.Generic;
using System.Linq;
using Telerik.Reporting.OpenXmlRendering;

namespace SW.InternalWeb.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ITransactionCodeService transactionCodeService;
        private readonly IReportingService _reportingService;

        public ReportsController(ITransactionCodeService transactionCodeService, IReportingService reportingService)
        {
            this.transactionCodeService = transactionCodeService;
            _reportingService = reportingService;
        }

        public async Task<IActionResult> Index()
        {
            ReportsViewModel rm = new ReportsViewModel();

            if (rm.ListCodes == null)
            {
                rm.ListCodes = new List<SelectListItem>();

                List<TransactionCode> ListCodes = await transactionCodeService.GetTransactionCodes();
                foreach (var c in ListCodes)
                {
                    var item = new SelectListItem();
                    item.Value = c.Code.ToString();
                    item.Text = string.Format("{0} - {1}", c.Code, c.Description);
                    rm.ListCodes.Add(item);
                }
            }

            if (rm.ListAccounts == null)
            {
                var accounts = new[] { "Collections", "Counselors" };
                rm.ListAccounts = new List<SelectListItem>();

                foreach (var a in accounts)
                {
                    var item = new SelectListItem();
                    item.Value = a.ToString();
                    item.Text = a;
                    rm.ListAccounts.Add(item);
                }
            }
            if (rm.KanPayListPayTypes == null)
            {
                var paytypes = new[] { "ALL", "CREDIT CARD", "E-CHECK" };
                rm.KanPayListPayTypes = new List<SelectListItem>();

                foreach (var p in paytypes)
                {
                    var item = new SelectListItem();
                    item.Value = p.ToString();
                    item.Text = p;
                    rm.KanPayListPayTypes.Add(item);
                }
            }
            return View(rm);
        }

        [HttpPost]
        public async Task<IActionResult> Transactions(ReportsViewModel vm, string[] listCodes)
        {            
            try 
            {
                if (listCodes == null)
                {
                    return RedirectToAction("Index", "Reports").WithInfo("", "Select Transaction Codes from the list");
                }
                if (vm.TransActionCode.StartDate == null)
                {
                    return RedirectToAction("Index", "Reports").WithInfo("", "Select a Transaction End Date");
                }
                if (vm.TransActionCode.EndDate == null)
                {
                    return RedirectToAction("Index", "Reports").WithInfo("", "Select a Transaction End Date");
                }

                var parameters = new Dictionary<string, object>
                {
                    {"transCode", listCodes},
                    {"startDate", vm.TransActionCode.StartDate.ToString()},
                    {"endDate", vm.TransActionCode.EndDate.ToString()},
                    {"customerNumber", vm.TransActionCode.CustomerNumber?.ToString()}
                };

                if (!vm.TransActionCode.SpreadSheet) 
                { 
                    var receipt = await _reportingService.GenerateReportPDF("Transactions", parameters);

                    return File(receipt, "application/pdf", "transactions_" + DateTime.Now + ".pdf");
                }
                else 
                {
                    var receipt = await _reportingService.GenerateReportXLS("Transactions", parameters);

                    return File(receipt, "application/xlsx", "transactions_" + DateTime.Now + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BatchBilling(ReportsViewModel vm)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"beg_datetime", vm.BatchBilling.BegDateTime},
                    {"end_datetime", vm.BatchBilling.EndDateTime},                    
                    {"customer_id", vm.BatchBilling.CustomerId}
                    //{"customer_id", null}
                };
                var receipt = await _reportingService.GenerateReportPDF("SW_Bill", parameters);

                return File(receipt, "application/pdf", "sw_bills_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }
    }
}
