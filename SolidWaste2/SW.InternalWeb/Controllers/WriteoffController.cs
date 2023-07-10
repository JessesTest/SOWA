using Common.Services.Common;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.Writeoff;

namespace SW.InternalWeb.Controllers;

[Authorize(Roles = "role.write-off")]
public class WriteoffController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personService;
    private readonly ITransactionService transactionService;

    public WriteoffController(
        ICustomerService customerService,
        IPersonEntityService personService,
        ITransactionService transactionService)
    {
        this.customerService = customerService;
        this.personService = personService;
        this.transactionService = transactionService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await transactionService.GetLatestTransactionsWithDelinquency();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? customerId)
    {
        if (customerId == null)
            return View(new WriteoffPaymentViewModel());

        var customer = await customerService.GetById(customerId.Value);
        if (customer == null)
            return View(new WriteoffPaymentViewModel())
                .WithDanger("Customer not found", "");

        var person = await personService.GetById(customer.Pe);
        if (person == null)
            return View(new WriteoffPaymentViewModel())
                .WithDanger("Customer is invalid", "");

        var lastTransaction = await transactionService.GetLatest(customer.CustomerId);

        var pastDue = lastTransaction == null ?
            0.00m : 
            await transactionService.Get90DaysPastDueAmount(customer.CustomerId);

        WriteoffPaymentViewModel model = new()
        {
            //Amount
            Collections = lastTransaction?.CollectionsBalance ?? 0.00m,
            //Comment
            Counselors = lastTransaction?.CounselorsBalance ?? 0.00m,
            CustomerId = customer.CustomerId,
            Name = person.FullName,
            PastDue90Days = pastDue,
            PaymentPlan = customer.PaymentPlan,
            //TransactionCode
            Uncollectable = lastTransaction?.UncollectableBalance ?? 0.00m
        };

        return View(model)
            .WithWarningWhen(customer.PaymentPlan, "Customer has a payment plan", "");
    }

    [HttpPost]
    public async Task<IActionResult> Create(WriteoffPaymentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model).WithDanger("There are errors on the form", "");

        var result = await transactionService.MakeDelinquencyPayment(model.CustomerId.Value, model.TransactionCode, model.Amount, model.Comment);
        if (result != null)
            return View(model).WithDanger(result, "");

        return RedirectToAction("Index", "Customer", new { model.CustomerId });
    }
}
