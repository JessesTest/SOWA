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
using DocumentFormat.OpenXml.Spreadsheet;
using Telerik.Barcode;

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

                var listCodes = await transactionCodeService.GetAll();
                foreach (var c in listCodes)
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
                    var report = await _reportingService.GenerateReportPDF("Transactions", parameters);

                    return File(report, "application/pdf", "transactions_" + DateTime.Now + ".pdf");
                }
                else 
                {
                    var report = await _reportingService.GenerateReportXLS("Transactions", parameters);

                    return File(report, "application/xlsx", "transactions_" + DateTime.Now + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransactionsbyDateRange(DateTime startDate, DateTime endDate, int customerId)
        {
            var parameters = new Dictionary<string, object>
                {
                    {"startDate", startDate.ToString()},
                    {"endDate", endDate.ToString()},
                    {"customerID", customerId.ToString()}
                };

            var report = await _reportingService.GenerateReportPDF("TransactionHistoryByCustomer", parameters);

            return File(report, "application/pdf", "transaction_history_" + customerId.ToString() + "_" + DateTime.Now + ".pdf");
        }

        [HttpPost]
        public async Task<IActionResult> Delinquency(bool exportToXls)
        {
            //The Telerik Report Delinquency uses a stored procedure sp_DelinquencyPlus that needs to be created within the SolidWaste db for all
            //environments

            try 
            {
                if (exportToXls)
                {
                    var report = await _reportingService.GenerateReportXLS("Delinquency");

                    return File(report, "application/xlsx", "delinquency_" + DateTime.Now + ".xlsx");
                }
                else
                {
                    var report = await _reportingService.GenerateReportPDF("Delinquency");

                    return File(report, "application/pdf", "delinquency_" + DateTime.Now + ".pdf");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }            
        }

        [HttpPost]
        public async Task<IActionResult> Balances(bool exportToXls)
        {
            //The Telerik Report DelinquencyPlus uses a stored procedure sp_DelinquencyPlus that needs to be created within the SolidWaste db for all
            //environments

            try
            {
                if (exportToXls)
                {
                    var report = await _reportingService.GenerateReportXLS("DelinquencyPlus");

                    return File(report, "application/xlsx", "balances_" + DateTime.Now + ".xlsx");
                }
                else
                {
                    var report = await _reportingService.GenerateReportPDF("DelinquencyPlus");

                    return File(report, "application/pdf", "aging_" + DateTime.Now + ".pdf");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> WriteOffRecommendation()
        {
            //The Telerik Report WriteOffRecommendations uses a stored procedure sp_WriteOffRecommendations that needs to be created within the SolidWaste db for all
            //environments

            try
            {
                var report = await _reportingService.GenerateReportPDF("WriteOffRecommendations");

                return File(report, "application/pdf", "write_off_recommendations_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RecyclingOnlyDelinquency()
        {
            //The Telerik Report RecyclingOnlyDelinquency uses a stored procedure sp_RecyclingOnlyDelinquency that needs to be created within the SolidWaste db for all
            //environments

            try
            {
                var report = await _reportingService.GenerateReportPDF("RecyclingOnlyDelinquency");

                return File(report, "application/pdf", "recycling_only_delinquency_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Revenue(string revenueYear)
        {
            try
            {
                if (revenueYear == null)
                {
                    revenueYear = DateTime.Now.Year.ToString();
                }

                var parameters = new Dictionary<string, object>
                {
                    {"SelectedYear", revenueYear}
                };

                var report = await _reportingService.GenerateReportPDF("Revenue", parameters);

                return File(report, "application/pdf", "revenue_" + revenueYear + "_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> WorkOrder(ReportsViewModel vm)
        {
            try
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();

                if (vm.WorkOrder.WorkOrderIdStart.HasValue)
                    parameters.Add(new KeyValuePair<string, object>("WorkOrderIdStart", vm.WorkOrder.WorkOrderIdStart.Value));
                else
                    parameters.Add(new KeyValuePair<string, object>("WorkOrderIdStart", 0));

                if (vm.WorkOrder.WorkOrderIdEnd.HasValue)
                    parameters.Add(new KeyValuePair<string, object>("WorkOrderIdEnd", vm.WorkOrder.WorkOrderIdEnd.Value));
                else
                    parameters.Add(new KeyValuePair<string, object>("WorkOrderIdEnd", int.MaxValue));

                if (!string.IsNullOrWhiteSpace(vm.WorkOrder.TransDateStart))
                    parameters.Add(new KeyValuePair<string, object>("TransDateStart", vm.WorkOrder.TransDateStart));
                else
                    parameters.Add(new KeyValuePair<string, object>("TransDateStart", "1753/01/01"));

                if (!string.IsNullOrWhiteSpace(vm.WorkOrder.TransDateEnd))
                    parameters.Add(new KeyValuePair<string, object>("TransDateEnd", vm.WorkOrder.TransDateEnd));
                else
                    parameters.Add(new KeyValuePair<string, object>("TransDateEnd", "9999/12/31"));

                if (!string.IsNullOrWhiteSpace(vm.WorkOrder.DriverInitials))
                    parameters.Add(new KeyValuePair<string, object>("DriverInitials", string.Format("%{0}%", vm.WorkOrder.DriverInitials)));
                else
                    parameters.Add(new KeyValuePair<string, object>("DriverInitials", "%%"));

                if (!string.IsNullOrWhiteSpace(vm.WorkOrder.CustomerType))
                    parameters.Add(new KeyValuePair<string, object>("CustomerType", string.Format("%{0}%", vm.WorkOrder.CustomerType)));
                else
                    parameters.Add(new KeyValuePair<string, object>("CustomerType", "%%"));

                if (!string.IsNullOrWhiteSpace(vm.WorkOrder.ContainerRouteStart))
                    parameters.Add(new KeyValuePair<string, object>("ContainerRouteStart", vm.WorkOrder.ContainerRouteStart));
                else
                    parameters.Add(new KeyValuePair<string, object>("ContainerRouteStart", 0));

                if (!string.IsNullOrWhiteSpace(vm.WorkOrder.ContainerRouteEnd))
                    parameters.Add(new KeyValuePair<string, object>("ContainerRouteEnd", vm.WorkOrder.ContainerRouteEnd));
                else
                    parameters.Add(new KeyValuePair<string, object>("ContainerRouteEnd", int.MaxValue));

                if (vm.WorkOrder.IncludeResolved)
                {
                    parameters.Add(new KeyValuePair<string, object>("ResolveDateStart", "1753/01/01"));
                    parameters.Add(new KeyValuePair<string, object>("ResolveDateEnd", "9999/12/31"));
                }
                else
                {
                    parameters.Add(new KeyValuePair<string, object>("ResolveDateStart", "9999/12/31"));
                    parameters.Add(new KeyValuePair<string, object>("ResolveDateEnd", "1753/01/01"));
                }
                
                var report = await _reportingService.GenerateReportPDF("WorkOrder", parameters);

                return File(report, "application/pdf", "work_order_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> WorkOrderByWorkOrderId(int workOrderId)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(new KeyValuePair<string, object>("CustomerType", "%%"));
            parameters.Add(new KeyValuePair<string, object>("WorkOrderIdStart", workOrderId));
            parameters.Add(new KeyValuePair<string, object>("WorkOrderIdEnd", workOrderId));
            parameters.Add(new KeyValuePair<string, object>("TransDateStart", "1753/01/01"));
            parameters.Add(new KeyValuePair<string, object>("TransDateEnd", "9999/12/31"));
            parameters.Add(new KeyValuePair<string, object>("DriverInitials", "%%"));
            parameters.Add(new KeyValuePair<string, object>("ContainerRouteStart", int.MinValue));
            parameters.Add(new KeyValuePair<string, object>("ContainerRouteEnd", int.MaxValue));
            parameters.Add(new KeyValuePair<string, object>("ResolveDateStart", "1753/01/01"));
            parameters.Add(new KeyValuePair<string, object>("ResolveDateEnd", "9999/12/31"));

            var report = await _reportingService.GenerateReportPDF("WorkOrder", parameters);

            return File(report, "application/pdf", "work_order_" + DateTime.Now + ".pdf");
        }

        [HttpPost]
        public async Task<IActionResult> ContractCharge(ReportsViewModel vm)
        {
            try 
            {
                var parameters = new Dictionary<string, object>
                {
                    {"contractCharge", vm.ContractChargeValue.ContractChargeZero}
                };

                if (vm.ContractChargeValue.SpreadSheet)
                {
                    var report = await _reportingService.GenerateReportXLS("ContractCharge", parameters);

                    return File(report, "application/xlsx", "contract_charge_" + DateTime.Now + ".xlsx");
                }
                else 
                {
                    var report = await _reportingService.GenerateReportPDF("ContractCharge", parameters);

                    return File(report, "application/pdf", "contract_charge_" + DateTime.Now + ".pdf");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PaymentPlanCustomers()
        {
            try 
            {
                var report = await _reportingService.GenerateReportPDF("PaymentPlanCustomers");

                return File(report, "application/pdf", "payment_plan_customers_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentPlanAgreement(int paymentPlanId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"paymentPlanId", paymentPlanId}
            };

            var report = await _reportingService.GenerateReportPDF("PaymentPlanAgreement", parameters);

            return File(report, "application/pdf", "payment_plan_agreement_" + DateTime.Now + ".pdf");
        }

        [HttpPost]
        public async Task<IActionResult> ActiveCustomerList()
        {
            try 
            {
                var report = await _reportingService.GenerateReportXLS("ActiveCustomerList");

                return File(report, "application/xlsx", "active_customer_list_" + DateTime.Now + ".xlsx");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Routes(bool RoutesExporttoXls)
        {
            try
            {
                if (RoutesExporttoXls)
                {
                    var report = await _reportingService.GenerateReportXLS("Routes");

                    return File(report, "application/xlsx", "routes_" + DateTime.Now + ".xlsx");
                }
                else
                {
                    var report = await _reportingService.GenerateReportPDF("Routes");

                    return File(report, "application/pdf", "routes_" + DateTime.Now + ".pdf");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> KanPay(ReportsViewModel vm)
        {
            try 
            {
                string[] listPayTypes = new string[2];
                switch (vm.KanPayActionCode.PayType)
                {
                    case "ALL":
                        listPayTypes[0] = "CC";
                        listPayTypes[1] = "ACH";
                        break;
                    default:
                        listPayTypes[0] = vm.KanPayActionCode.PayType.Contains("CREDIT") ? "CC" : "ACH";
                        break;
                }

                if (vm.KanPayActionCode.StartDate == null)
                {
                    return RedirectToAction("Index", "Reports").WithDanger("", "Select KanPay Start Date");
                }
                if (vm.KanPayActionCode.EndDate == null)
                {
                    return RedirectToAction("Index", "Reports").WithDanger("", "Select KanPay End Date");
                }

                var parameters = new Dictionary<string, object>
                {
                    {"payType", listPayTypes},
                    {"startDate", vm.KanPayActionCode.StartDate.ToString()},
                    {"endDate", vm.KanPayActionCode.EndDate.ToString()},
                    {"customerNumber", vm.KanPayActionCode.CustomerNumber?.ToString()}
                };

                if (!vm.KanPayActionCode.SpreadSheet)
                {
                    var report = await _reportingService.GenerateReportPDF("KanPay", parameters);

                    return File(report, "application/pdf", "kanpay_" + DateTime.Now + ".pdf");
                }
                else
                {
                    var report = await _reportingService.GenerateReportXLS("KanPay", parameters);

                    return File(report, "application/xlsx", "kanpay_" + DateTime.Now + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }            
        }

        [HttpPost]
        public async Task<IActionResult> Delinquents(ReportsViewModel vm)
        {
            try 
            {
                if (vm.DelinquentAccount.StartDate == null)
                {
                    return RedirectToAction("Index", "Reports").WithDanger("", "Select Delinquent Start Date");
                }
                if (vm.DelinquentAccount.EndDate == null)
                {
                    return RedirectToAction("Index", "Reports").WithDanger("", "Select Delinquent End Date");
                }

                var parameters = new Dictionary<string, object>
                {
                    {"startDate", vm.DelinquentAccount.StartDate.ToString()},
                    {"endDate", vm.DelinquentAccount.EndDate.ToString()},
                    {"customerNumber", vm.DelinquentAccount.CustomerNumber?.ToString()}
                };

                if (vm.DelinquentAccount.Account == "Collections")
                {
                    if (vm.DelinquentAccount.SpreadSheet)
                    {
                        var report = await _reportingService.GenerateReportXLS("DelinquentCollections", parameters);

                        return File(report, "application/xlsx", "delinquent_collections_" + DateTime.Now + ".xlsx");
                    }
                    else
                    {
                        var report = await _reportingService.GenerateReportPDF("DelinquentCollections", parameters);

                        return File(report, "application/pdf", "delinquent_collections_" + DateTime.Now + ".pdf");
                    }
                }
                else
                {
                    if (vm.DelinquentAccount.SpreadSheet)
                    {
                        var report = await _reportingService.GenerateReportXLS("DelinquentCounselors", parameters);

                        return File(report, "application/xlsx", "delinquent_counselors_" + DateTime.Now + ".xlsx");
                    }
                    else
                    {
                        var report = await _reportingService.GenerateReportPDF("DelinquentCounselors", parameters);

                        return File(report, "application/pdf", "delinquent_counselors_" + DateTime.Now + ".pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }      
        }

        //SOWA-56: SW Batch Billing Telerik Report Testing
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
                };
                var report = await _reportingService.GenerateReportPDF("SW_Bill", parameters);

                return File(report, "application/pdf", "sw_bills_" + DateTime.Now + ".pdf");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Reports").WithDanger("", ex.Message);
            }
        }
    }
}
