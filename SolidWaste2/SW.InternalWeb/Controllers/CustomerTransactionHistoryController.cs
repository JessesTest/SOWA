using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.InternalWeb.Models.CustomerTransactionHistory;
using SW.Reporting.Services;

namespace SW.InternalWeb.Controllers;

public class CustomerTransactionHistoryController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personService;
    private readonly ITransactionService transactionService;
    private readonly IBillMasterService billMasterService;
    private readonly IBillBlobService billBlobService;
    private readonly IReportingService reportingService;

    public CustomerTransactionHistoryController(
        ICustomerService customerService,
        IPersonEntityService personService,
        ITransactionService transactionService,
        IBillMasterService billMasterService,
        IBillBlobService billBlobService,
        IReportingService reportingService)
    {
        this.customerService = customerService;
        this.personService = personService;
        this.transactionService = transactionService;
        this.billMasterService = billMasterService;
        this.billBlobService = billBlobService;
        this.reportingService = reportingService;
    }

    public async Task<IActionResult> Index(int customerId)
    {
        var customer = await customerService.GetById(customerId);

        if (customerId == customer.LegacyCustomerId)
        {
            return RedirectToAction("Index", new { customerId = customer.CustomerId });
        }

        var personEntity = await personService.GetById(customer.Pe);

        if (personEntity.Pab.HasValue && personEntity.Pab.Value)
        {
            ModelState.AddModelError("warning", "Account has undeliverable address.");
        }
        var transactions = (await transactionService.GetByCustomer(customer.CustomerId, false))
            .OrderByDescending(t => t.Sequence)
            .OrderByDescending(m => m.AddDateTime)
            .ToList();

        CustomerTransactionHistoryViewModel vm = new()
        {
            CustomerID = customerId,
            endDate = DateTime.Today.AddDays(1)
        };

        if (transactions.Count >= 10)
        {
            vm.startDate = transactions[9].AddDateTime;
        }
        if (transactions.Count <= 0)
        {
            vm.startDate = DateTime.Now;
        }
        else
        {
            vm.startDate = transactions[transactions.Count - 1].AddDateTime;
        }

        TempData["FullName"] = personEntity.FullName;

        return View(vm)
            .WithInfoWhen(customer.PaymentPlan, "", "Customer has a payment plan.");
    }

    public async Task<IActionResult> IndexJson(int customerID)
    {
        var customer = await customerService.GetById(customerID);

        var listDM = (await transactionService.GetByCustomer(customer.CustomerId, false))
            .OrderByDescending(t => t.Sequence)
            .OrderByDescending(t => t.AddDateTime)
            .ToList();

        List<CustomerTransactionViewModel> transactions = new List<CustomerTransactionViewModel>();
        foreach (var t in listDM)
        {
            CustomerTransactionViewModel tran = new()
            {
                AddDateTime = t.AddDateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                BatchID = "",
                CheckNumber = t.CheckNumber?.ToString(),
                CollectionsAmount = t.CollectionsAmount.ToString("0.00"),
                CollectionsBalance = t.CollectionsBalance.ToString("0.00"),
                Comments = t.Comment,
                ContractCharge = customer.ContractCharge?.ToString() ?? "",
                CounselorsAmount = t.CounselorsAmount.ToString("0.00"),
                CounselorsBalance = t.CounselorsBalance.ToString("0.00").ToString(),
                Description = t.Comment ?? t.TransactionCode.Description,
                Sequence = t.Sequence.ToString(),
                TransactionAmount = t.TransactionAmt.ToString("0.00"),
                TransactionBalance = t.TransactionBalance.ToString("0.00"),
                UncollectableAmount = t.UncollectableAmount?.ToString("0.00") ?? "0.00",
                UncollectableBalance = t.UncollectableBalance?.ToString("0.00") ?? "0.00",
                UserID = t.AddToi,
                WorkOrder = t.WorkOrder
            };
            transactions.Add(tran);

            var bm = await billMasterService.GetByTransaction(t.Id);
            if (bm == null)
            {
                continue;
            }
            tran.BillMaster = new CustomerBillMasterViewModel
            {
                AddDateTime = bm.AddDateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                AddToi = bm.AddToi,
                BillingAddressCityStateZip = bm.BillingAddressCityStateZip,
                BillingAddressStreet = bm.BillingAddressStreet,
                BillingPeriodBegDate = bm.BillingPeriodBegDate.ToString(),
                BillingPeriodEndDate = bm.BillingPeriodEndDate.ToString(),
                BilllingName = bm.BillingName,
                BillMasterId = bm.BillMasterId.ToString(),
                BillMessage = bm.BillMessage,
                BillServiceAddress = bm.BillServiceAddresses.Select(sa => new CustomerBillServiceAddressViewModel
                {
                    BillContainers = sa.BillContainerDetails.Select(d => new CustomerBillContainerDetailViewModel
                    {
                        BillContainerDetailId = d.BillContainerDetailId.ToString(),
                        BillServiceAddressId = d.BillServiceAddressId.ToString(),
                        ContainerCancelDate = d.ContainerCancelDate?.ToString("d"),
                        ContainerCharge = d.ContainerCharge.ToString("0.00"),
                        ContainerDescription = d.ContainerDescription,
                        ContainerEffectiveDate = d.ContainerEffectiveDate.ToString("d"),
                        ContainerType = d.ContainerType,
                        DaysProratedMessage = d.DaysProratedMessage,
                        DaysService = d.DaysService,
                        RateAmount = d.RateAmount.ToString("0.00"),
                        RateDescription = d.RateDescription
                    }).ToList(),
                    BillMasterId = sa.BillMasterId.ToString(),
                    BillServiceAddressId = sa.BillServiceAddressId.ToString(),
                    ServiceAddressName = sa.ServiceAddressName,
                    ServiceAddressStreet = sa.ServiceAddressStreet
                }).ToList(),
                ChgDateTime = bm.ChgDateTime.ToString(),
                ChgToi = bm.ChgToi,
                ContainerCharges = bm.ContainerCharges.ToString("0.00"),
                ContractCharge = bm.ContractCharge?.ToString("0.00") ?? "N/A",
                CustomerID = bm.CustomerId.ToString(),
                CustomerType = bm.CustomerType,
                DelDateTime = bm.DelDateTime?.ToString(),
                DeleteFlag = bm.DeleteFlag.ToString(),
                DelToi = bm.DelToi,
                FinalBill = bm.FinalBill.ToString(),
                PastDueAmt = bm.PastDueAmt.ToString("0.00"),
                TransactionID = bm.TransactionId.ToString()
            };
        }

        return Json(new { data = transactions });
    }

    public async Task<IActionResult> DownloadBill(int transactionId)
    {
        var transaction = await transactionService.GetById(transactionId);
        if (transaction == null)
            return NotFound();

        if (transaction.AddDateTime < Convert.ToDateTime("2016-07-01"))
        {
            var bill_trans_hist_beg_datetime = await transactionService.GetLastBillTranDateTime(
                transaction.CustomerId,
                transaction.AddDateTime);

            Dictionary<string, object> parameters = new()
            {
                { "beg_datetime", bill_trans_hist_beg_datetime.ToString("MM/dd/yyyy HH:mm:ss.FFF") },
                { "end_datetime", transaction.AddDateTime.ToString("MM/dd/yyyy HH:mm:ss.FFF") },
                { "customer_id", transaction.CustomerId }
            };

            var pdfBytes = await reportingService.GenerateReportPDF("SW_Bill", parameters);

            return File(pdfBytes, "application/pdf", "sw_bill.pdf");
        }
        else
        {
            var billBlob = await billBlobService.GetByTransactionId(transactionId);
            if (billBlob == null)
                return NotFound();

            return File(billBlob.BillFile, "application/pdf", "sw_bill.pdf");
        }
    }
}
