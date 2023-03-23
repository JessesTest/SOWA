using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.DM;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models;
using System.Diagnostics;

namespace SW.ExternalWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICustomerService customerService;
        private readonly IBillMasterService billMasterService;
        private readonly ITransactionService transactionService;
        private readonly IPersonEntityService personEntityService;
        private readonly IBillContainerService billContainerService;
        private readonly IBillingSummaryService billingSummaryService;
        private readonly IPaymentPlanService paymentPlanService;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            ICustomerService customerService,
            IBillMasterService billMasterService,
            ITransactionService transactionService,
            IPersonEntityService personEntityService,
            IBillContainerService billContainerService,
            IBillingSummaryService billingSummaryService,
            IPaymentPlanService paymentPlanService)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.customerService = customerService;
            this.billMasterService = billMasterService;
            this.transactionService = transactionService;
            this.personEntityService = personEntityService;
            this.billContainerService = billContainerService;
            this.billingSummaryService = billingSummaryService;
            this.paymentPlanService = paymentPlanService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var person = await personEntityService.GetById(user.UserId);
            var customer = await customerService.GetByPE(person.Id);
            var customerId = customer.CustomerId;

            AccountHomeViewModel vm = new()
            {
                AccountSummaryDueDate = DateTime.Today
            };

            if (customer != null)
            {
                vm.CustomerId = customerId.ToString();

                var currentBillMaster = await billMasterService.GetMostRecentBillMaster(customerId);
                if (currentBillMaster != null)
                {
                    vm.BillingAddress = string.Format("{0} {1}", currentBillMaster.BillingAddressStreet, currentBillMaster.BillingAddressCityStateZip);
                    vm.AccountSummaryPeriodBeginDate = currentBillMaster.BillingPeriodBegDate;
                    vm.AccountSummaryPeriodEndDate = currentBillMaster.BillingPeriodEndDate;
                    vm.AccountSummaryDueDate = DateTime.Parse(string.Format("{0}/{1}/{2}", currentBillMaster.AddDateTime.Year, currentBillMaster.AddDateTime.Month, 15));
                    vm.AccountSummaryAnyPayments = await billingSummaryService.GetTotalPaymentsForCustomerInDateRange(customerId, currentBillMaster.BillingPeriodBegDate, currentBillMaster.BillingPeriodEndDate);
                    vm.AccountSummaryAnyAdjustments = await billingSummaryService.GetTotalAdjustmentsForCustomerInDateRange(customerId, currentBillMaster.BillingPeriodBegDate, currentBillMaster.BillingPeriodEndDate);
                    vm.AccountSummaryNewCharges = await billingSummaryService.GetTotalNewChargesForCustomerInDateRange(customerId, currentBillMaster.BillingPeriodBegDate, currentBillMaster.BillingPeriodEndDate);

                    var currentBillMasterTransaction = await transactionService.GetById(currentBillMaster.TransactionId);
                    if (currentBillMasterTransaction != null)
                    {
                        vm.AccountSummaryTotalDue = currentBillMasterTransaction.TransactionBalance;
                        vm.CurrentBillTransactionId = currentBillMasterTransaction.Id.ToString();
                    }

                    var previousBillMaster = await billMasterService.GetPreviousBillMaster(customerId);
                    if (previousBillMaster != null)
                    {
                        var previousBillMasterTransaction = await transactionService.GetById(previousBillMaster.TransactionId);
                        vm.AccountSummaryPreviousBalance = previousBillMasterTransaction.TransactionBalance;
                    }
                    else
                    {
                        vm.AccountSummaryPreviousBalance = 0m;
                    }
                }
            }
            return PartialView(vm);
        }

        public IActionResult MakeError()
        {
            logger.LogError("MakeError");
            throw new NotImplementedException("Just testing");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            //this is the error that would have been redirected here by the global handler
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                logger.LogError("{userName} experienced {errorMessage} during transaction {transactionId}", User.GetNameOrEmail(), exceptionFeature.Error.Message, Activity.Current?.Id);
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> BillSummary()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var person = await personEntityService.GetById(user.UserId);
            var customer = await customerService.GetByPE(person.Id);
            var customerId = customer.CustomerId;

            HomePageViewModel vm = new();

            var billMaster = await billMasterService.GetMostRecentBillMaster(customerId);
            if(billMaster != null)
            {
                var trans = await transactionService.GetById(billMaster.TransactionId);
                vm.MostRecentBillAmt = trans.TransactionAmt;
            }

            var paymentPlan = await paymentPlanService.GetActiveByCustomer(customerId);
            vm.DOIHAVECOUNSELORS = (await transactionService.GetCounselorsBalance(customerId)) > 0m;

            vm.CustomerId = customerId;
            vm.PastDueBalance = await transactionService.GetPastDueAmount(customerId);
            vm.CurrentBalance = await transactionService.GetCurrentBalance(customerId);
            vm.PPBalance = paymentPlan?.Details.FirstOrDefault(m => m.Paid != true).PaymentTotal;

            return View(vm);
        }

        public async Task<IActionResult> BillHistory()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var person = await personEntityService.GetById(user.UserId);
            var customer = await customerService.GetByPE(person.Id);
            var customerId = customer.CustomerId;

            HomePageViewModel vm = new();

            var billMaster = await billMasterService.GetMostRecentBillMaster(customerId);
            if(billMaster != null)
            {
                var trans = await transactionService.GetById(billMaster.TransactionId);
                if (trans != null)
                    vm.MostRecentBillAmt = trans.TransactionAmt;
            }

            var paymentPlan = await paymentPlanService.GetActiveByCustomer(customerId);
            vm.DOIHAVECOUNSELORS = (await transactionService.GetCounselorsBalance(customerId)) > 0m;

            vm.CustomerId = customerId;
            vm.PastDueBalance = await transactionService.GetPastDueAmount(customerId);
            vm.CurrentBalance = await transactionService.GetCurrentBalance(customerId);
            vm.PPBalance = paymentPlan?.Details.FirstOrDefault(m => m.Paid != true).PaymentTotal;

            return View(vm);
        }

        public async Task<IActionResult> ServiceDetail()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var person = await personEntityService.GetById(user.UserId);
            var customer = await customerService.GetByPE(person.Id);
            var customerId = customer.CustomerId;

            return View(customerId);
        }

        public async Task<IActionResult> HomeMenu(string controller, string action)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var person = await personEntityService.GetById(user.UserId);
            var customer = await customerService.GetByPE(person.Id);
            var customerId = customer.CustomerId;

            AccountHomeMenuViewModel vm = new();

            var billMaster = await billMasterService.GetMostRecentBillMaster(customerId);
            if (billMaster != null)
            {
                var trans = await transactionService.GetById(billMaster.TransactionId);
                if (trans != null)
                {
                    vm.MostRecentBillAmt = trans.TransactionAmt;
                }
                decimal activ = 
                    (await billingSummaryService.GetTotalPaymentsForCustomerInDateRange(customerId, billMaster.BillingPeriodEndDate, DateTime.Now))
                    + (await billingSummaryService.GetTotalAdjustmentsForCustomerInDateRange(customerId, billMaster.BillingPeriodEndDate, DateTime.Now));

                vm.DueDate = DateTime.Parse(string.Format("{0}/{1}/{2}", billMaster.AddDateTime.Year, billMaster.AddDateTime.Month, 15));
                vm.ActivitySinceLastBill = activ;
            }
            else
            {
                vm.DueDate = DateTime.Today;
            }

            var paymentPlan = await paymentPlanService.GetActiveByCustomer(customerId);
            vm.PPFlag = customer.PaymentPlan;
            vm.PPBalance = paymentPlan?.Details.FirstOrDefault(m => m.Paid != true).PaymentTotal;

            vm.AccountNumber = customerId.ToString();
            vm.CurrentBalance = await transactionService.GetCurrentBalance(customerId);
            vm.PastDueBalance = await transactionService.GetPastDueAmount(customerId);
            vm.CurrentController = controller;
            vm.CurrentAction = action;
            vm.CustomerType = customer.CustomerType;

            return PartialView("_Menu", vm);
        }

        [HttpGet]
        public async Task<IActionResult> TransactionsJson()
        {
            var user = await userManager.FindByIdAsync(User.GetUserId());
            var person = await personEntityService.GetById(user.UserId);
            var customer = await customerService.GetByPE(person.Id);
            var customerId = customer.CustomerId;

            var billMaster = await billMasterService.GetMostRecentBillMaster(customerId);
            if (billMaster == null)
                return Json(new { data = Array.Empty<string>() });

            IEnumerable<Transaction> listDM = (await transactionService.GetByCustomer(customerId, false))
                .Where(m => m.AddDateTime >= billMaster.BillingPeriodBegDate && m.Id <= billMaster.TransactionId);

            List<CustomerTransactionViewModel> transactions = new ();
                
            foreach (Transaction t in listDM)
            {
                CustomerTransactionViewModel tran = new()
                {
                    //Activity = t.Activity
                    AddDateTime = t.AddDateTime.ToString(),
                    //Balance = 
                    //BatchID = 
                    //BillMaster = t.BillMasters
                    CheckNumber = t.CheckNumber?.ToString(),
                    CollectionsAmount = t.CollectionsAmount.ToString("0.00"),
                    CollectionsBalance = t.CollectionsBalance.ToString("0.00"),
                    //Comments =
                    //Container = t.Container
                    //ContractCharge = 
                    CounselorsAmount = t.CounselorsAmount.ToString("0.00"),
                    CounselorsBalance = t.CounselorsBalance.ToString("0.00"),
                    //Description =
                    Sequence = t.Sequence.ToString(),
                    TransactionAmount = t.TransactionAmt.ToString("0.00"),
                    TransactionBalance = t.TransactionBalance.ToString("0.00"),
                    //UserID =
                    WorkOrder = t.WorkOrder
                };
                tran.ContractCharge = customer.ContractCharge?.ToString("0.00") ?? "";
                if (tran.Description.Contains("MONTHLY BILL"))
                {
                    tran.Activity = t.Comment;
                }
                else
                {
                    tran.Activity = t.TransactionCode.Description;
                }
                if (tran.Activity == "PAYMENT")
                {
                    tran.Activity += " - Thank You!";
                }
                tran.Balance = t.TransactionBalance.ToString();

                transactions.Add(tran);

                var bm =  await billMasterService.GetByTransaction(t.Id);
                if (bm == null)
                {
                    continue;
                }
                tran.BillMaster = new CustomerBillMasterViewModel
                {
                    AddDateTime = bm.AddDateTime.ToString(),
                    AddToi = bm.AddToi,
                    BillingAddressCityStateZip = bm.BillingAddressCityStateZip,
                    BillingAddressStreet = bm.BillingAddressStreet,
                    BillingPeriodBegDate = bm.BillingPeriodBegDate.ToString(),
                    BillingPeriodEndDate = bm.BillingPeriodEndDate.ToString(),
                    BilllingName = bm.BillingName,
                    BillMasterId = bm.BillMasterId.ToString(),
                    BillMessage = bm.BillMessage,
                    //BillServiceAddress = bm.BillServiceAddresses
                    ChgDateTime = bm.ChgDateTime?.ToString(),
                    ChgToi = bm.ChgToi,
                    ContainerCharges = bm.ContainerCharges.ToString("0.00"),
                    ContractCharge = bm.ContractCharge?.ToString("0.00"),
                    CustomerID = bm.CustomerId.ToString(),
                    CustomerType = bm.CustomerType,
                    DelDateTime = bm.DelDateTime?.ToString(),
                    DeleteFlag = bm.DeleteFlag.ToString(),
                    DelToi = bm.DelToi,
                    FinalBill = bm.FinalBill.ToString(),
                    PastDueAmt = bm.PastDueAmt.ToString("0.00"),
                    TransactionID = bm.TransactionId.ToString()
                };
                tran.BillMaster.BillServiceAddress = new List<CustomerBillServiceAddressViewModel>();
                foreach(var b in bm.BillServiceAddresses)
                {
                    b.BillContainerDetails = await billContainerService.GetByBillServiceAddress(b.BillServiceAddressId);

                    tran.BillMaster.BillServiceAddress.Add(new CustomerBillServiceAddressViewModel
                    {
                        BillContainers = new(),
                        BillMasterId = b.BillMasterId.ToString(),
                        BillServiceAddressId = b.BillServiceAddressId.ToString(),
                        ServiceAddressName = b.ServiceAddressName,
                        ServiceAddressStreet = b.ServiceAddressStreet
                    });


                    List<CustomerBillContainerDetailViewModel> bcd = new();
                    
                    foreach (var c in b.BillContainerDetails)
                    {
                        bcd.Add(new CustomerBillContainerDetailViewModel
                        {
                            BillContainerDetailId = c.BillContainerDetailId.ToString(),
                            BillServiceAddressId = c.BillServiceAddressId.ToString(),
                            ContainerCancelDate = c.ContainerCancelDate?.ToString(),
                            ContainerCharge = c.ContainerCharge.ToString("0.00"),
                            ContainerDescription = c.ContainerDescription,
                            ContainerEffectiveDate = c.ContainerEffectiveDate.ToString(),
                            ContainerType = c.ContainerType,
                            DaysProratedMessage = c.DaysProratedMessage,
                            DaysService = c.DaysService,
                            RateAmount = c.RateAmount.ToString("0.00"),
                            RateDescription = c.RateDescription
                        });
                    }

                    tran.BillMaster.BillServiceAddress.Last().BillContainers = bcd;
                }

                 
                List<CustomerBillServiceAddressViewModel> bsa = new();
                foreach (var b in bm.BillServiceAddresses)
                {
                    b.BillContainerDetails = await billContainerService.GetByBillServiceAddress(b.BillServiceAddressId);
                    bsa.Add(new CustomerBillServiceAddressViewModel
                    {
                        BillContainers = new(),
                        BillMasterId = b.BillMasterId.ToString(),
                        BillServiceAddressId = b.BillServiceAddressId.ToString(),
                        ServiceAddressName = b.ServiceAddressName,
                        ServiceAddressStreet = b.ServiceAddressStreet
                    });
                    List<CustomerBillContainerDetailViewModel> bcd = new List<CustomerBillContainerDetailViewModel>();
                    foreach (var c in b.BillContainerDetails)
                    {
                        bcd.Add(new CustomerBillContainerDetailViewModel
                        {
                            BillContainerDetailId = c.BillContainerDetailId.ToString(),
                            BillServiceAddressId = c.BillServiceAddressId.ToString(),
                            ContainerCancelDate = c.ContainerCancelDate.ToString(),
                            ContainerCharge = c.ContainerCharge.ToString("0.00"),
                            ContainerDescription = c.ContainerDescription,
                            ContainerEffectiveDate = c.ContainerEffectiveDate.ToString(),
                            ContainerType = c.ContainerType,
                            DaysProratedMessage = c.DaysProratedMessage,
                            DaysService = c.DaysService,
                            RateAmount = c.RateAmount.ToString("0.00"),
                            RateDescription = c.RateDescription
                        });
                    }
                    bsa.Last().BillContainers = bcd;
                }
                tran.BillMaster.BillServiceAddress = bsa;
            }
            return Json(new { data = transactions });
        }
    }
}
