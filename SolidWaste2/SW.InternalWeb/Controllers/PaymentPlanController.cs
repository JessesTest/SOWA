using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.PaymentPlans;

namespace SW.InternalWeb.Controllers;

public class PaymentPlanController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPaymentPlanService paymentPlanService;
    private readonly IPersonEntityService personEntityService;
    private readonly IBillingSummaryService billingSummaryService;
    private readonly ITransactionService transactionService;
    private readonly ITransactionCodeService transactionCodeService;

    public PaymentPlanController(
        ICustomerService customerService,
        IPaymentPlanService paymentPlanService,
        IPersonEntityService personEntityService,
        IBillingSummaryService billingSummaryService,
        ITransactionService transactionService,
        ITransactionCodeService transactionCodeService)
    {
        this.customerService = customerService;
        this.paymentPlanService = paymentPlanService;
        this.personEntityService = personEntityService;
        this.billingSummaryService = billingSummaryService;
        this.transactionService = transactionService;
        this.transactionCodeService = transactionCodeService;
    }

    public async Task<IActionResult> Index(int customerId)
    {
        var paymentPlans = await paymentPlanService.GetByCustomer(customerId, true);
        return View(new PaymentPlansViewModel
        {
            CustomerId = customerId,
            PaymentPlans = paymentPlans
        });
    }

    #region Create

    [HttpGet]
    public async Task<IActionResult> Create(int customerId)
    {
        var customer = await customerService.GetById(customerId);
        if (customer == null || customer.DelDateTime != null)
            return RedirectToAction("Index", "Home")
                .WithDanger("Customer not found", "");

        var paymentPlan = await paymentPlanService.GetActiveByCustomer(customer.CustomerId, true);
        if (paymentPlan != null)
            return RedirectToAction(nameof(Index), new { customerId })
                .WithDanger("Account already has a payment plan", "");

        var summary = await billingSummaryService.GetBillingSummaryForPaymentPlan(customerId);
        var pe = await personEntityService.GetById(customer.Pe);
        var remainingBalance = await transactionService.GetRemainingBalanceFromLastBill(customer.CustomerId);

        PaymentPlanViewModel model = new()
        {
            CustomerId = customerId,
            CustomerType = customer.CustomerType,
            Months = 6,
            TotalDue = remainingBalance,
            FirstPaymentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15).AddMonths(1),
            FullName = pe.FullName,
            MonthlyTotal = summary.Total
        };
        model.MonthlyPayment = model.TotalDue / model.Months + model.MonthlyTotal;

        return View(model)
            .WithDangerWhen(remainingBalance <= 0, "Account has no past due charges", "");
    }

    [HttpPost]
    public async Task<IActionResult> Create(PaymentPlanViewModel model)
    {
        var customer = await customerService.GetById(model.CustomerId);
        if (customer == null || customer.DelDateTime != null)
            return RedirectToAction("Index", "Home").WithDanger("Customer not found", "");

        var paymentPlan = await paymentPlanService.GetActiveByCustomer(customer.CustomerId, true);
        if(paymentPlan != null)
        {
            return RedirectToAction(nameof(Index)).WithDanger("Account already has a payment plan", "");
        }
        if (model.TotalDue <= 0)
        {
            return RedirectToAction(nameof(Index)).WithDanger("Account has no past due charges", "");
        }

        if (!ModelState.IsValid)
        {
            return View(model).WithDanger("There were errors on the form", "");
        }

        var p = new PaymentPlan
        {
            AddDateTime = DateTime.Now,
            AddToi = User.Identity.Name,
            CustomerId = model.CustomerId,
            CustomerType = model.CustomerType,
            Details = new List<PaymentPlanDetail>(),
            Months = model.Months
        };

        var dueDate = model.FirstPaymentDate;

        for (var i = 0; i < model.Months; i++)
        {
            var d = new PaymentPlanDetail
            {
                AddDateTime = DateTime.Now,
                AddToi = User.Identity.Name,
                Amount = Math.Round(model.TotalDue / model.Months, 2),
                PaymentTotal = Math.Round(model.TotalDue / model.Months + model.MonthlyTotal, 2),
                Caneled = false,
                DueDate = dueDate,
                BillDate = new DateTime(dueDate.Year, dueDate.Month, 1)
            };
            p.Details.Add(d);
            dueDate = dueDate.AddMonths(1);
        }

        var paymentPlanAmount = Math.Round(model.TotalDue / model.Months, 2);
        var paymentPlanTotal = paymentPlanAmount * model.Months;
        var overAmount = paymentPlanTotal - model.TotalDue;
        if (overAmount > 0)
        {
            var overMonths = (int)(overAmount * 100);
            var overDetails = p.Details.Skip(p.Details.Count - overMonths).ToList();
            foreach (var d in overDetails)
            {
                d.Amount -= 0.01m;
                d.PaymentTotal -= 0.01m;
            }
        }
        else if (overAmount < 0)
        {
            var underMonths = (int)(overAmount * 100);
            var underDetails = p.Details.Take(-underMonths).ToList();
            foreach (var d in underDetails)
            {
                d.Amount += 0.01m;
                d.PaymentTotal += 0.01m;
            }
        }

        await paymentPlanService.Add(p);

        var latest = await transactionService.GetLatest(model.CustomerId);
        var transactionCode = await transactionCodeService.GetByCode("PP");
        var startPP = new Transaction
        {
            AddDateTime = DateTime.Now,
            AddToi = User.Identity.Name,
            CollectionsBalance = latest.CollectionsBalance,
            UncollectableBalance = latest.UncollectableBalance,
            Comment = "Start payment plan",
            CounselorsBalance = latest.CounselorsBalance,
            CustomerId = latest.CustomerId,
            CustomerType = latest.CustomerType,
            TransactionBalance = latest.TransactionBalance,
            TransactionCodeId = transactionCode.TransactionCodeId
        };
        await transactionService.AddTransaction(startPP);

        customer.PaymentPlan = true;
        await customerService.Update(customer);

        return RedirectToAction("Index", new { customerId = model.CustomerId });
    }

    #endregion

    #region Edit

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var pp = await paymentPlanService.GetById(id);
        if (pp == null || pp.DelFlag)
            return RedirectToAction("Index", "Home").WithDanger("Payment plan not found", "");

        var customer = await customerService.GetById(pp.CustomerId);
        if(customer == null)
            return RedirectToAction("Index", "Home").WithDanger("Customer not found", "");

        if (pp.Canceled)
            return RedirectToAction(nameof(Index), new { customerId = customer.CustomerId }).WithDanger("Payment plan was canceled", "");

        var pe = await personEntityService.GetById(customer.Pe);

        var model = new PaymentPlanViewModel
        {
            AddDateTime = pp.AddDateTime,
            AddToi = pp.AddToi,
            Canceled = pp.Canceled,
            ChgDateTime = pp.ChgDateTime,
            ChgToi = pp.ChgToi,
            CustomerId = pp.CustomerId,
            CustomerType = pp.CustomerType,
            FirstPaymentDate = pp.Details.First().DueDate,
            FullName = pe.FullName,
            Id = pp.Id,
            MonthlyPayment = pp.Details.Last().Amount,
            MonthlyTotal = pp.Details.Last().PaymentTotal,
            Months = pp.Months,
            Status = pp.Status
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PaymentPlanViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model).WithDanger("There were errors on the form", "");

        var pp = await paymentPlanService.GetById(model.Id);
        if (pp == null || pp.DelFlag)
            return RedirectToAction("Index", "Home").WithDanger("Payment plan not found", "");

        var customer = await customerService.GetById(pp.CustomerId);
        if (customer == null)
            return RedirectToAction("Index", "Home").WithDanger("Customer not found", "");

        if (model.Canceled && customer.PaymentPlan)
        {
            customer.PaymentPlan = false;
            customer.ChgDateTime = DateTime.Now;
            customer.ChgToi = User.GetNameOrEmail();
            await customerService.Update(customer);
        }

        pp.ChgDateTime = DateTime.Now;
        pp.ChgToi = User.GetNameOrEmail();
        pp.Canceled = model.Canceled;
        
        foreach(var d in pp.Details)
        {
            if (d.Paid == true)
                continue;
            d.Caneled = model.Canceled;
            d.ChgDateTime = DateTime.Now;
            d.ChgToi = User.Identity.Name;
        }
        await paymentPlanService.Update(pp);

        return View(model).WithSuccess("Payment plan updated", "");
    }

    #endregion
}
