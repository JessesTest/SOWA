using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.InternalWeb.Models.Collections;

namespace SW.InternalWeb.Controllers;

public class CollectionsController : Controller
{
    private readonly ITransactionService transactionService;
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personEntityService;

    public CollectionsController(
        ITransactionService transactionService,
        ICustomerService customerService,
        IPersonEntityService personEntityService)
    {
        this.transactionService = transactionService;
        this.customerService = customerService;
        this.personEntityService = personEntityService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await transactionService.GetLatestTransactionsWithDelinquency();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Payment(int? customerId, string transactionCode)
    {
        CollectionsPaymentViewModel model = new();

        if (customerId.HasValue)
        {
            var customer = await customerService.GetById(customerId.Value);
            if (customer == null || customer.DelDateTime.HasValue)
                return NotFound();

            var personEntity = await personEntityService.GetById(customer.Pe);
            var latestTransaction = await transactionService.GetLatest(customer.CustomerId);

            model.CustomerId = customer.CustomerId;
            model.Name = personEntity.FullName;
            model.PaymentPlan = customer.PaymentPlan;
            model.Collections = latestTransaction.CollectionsBalance;
            model.Counselors = latestTransaction.CounselorsBalance;
            model.Uncollectable = latestTransaction.UncollectableBalance.GetValueOrDefault();

            if (!string.IsNullOrWhiteSpace(transactionCode))
                model.TransactionCode = transactionCode;
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Payment(CollectionsPaymentViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ArgumentException("There are errors on the form");

            var customer = await customerService.GetById(model.CustomerId.GetValueOrDefault());
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var result = await transactionService.MakeDelinquencyPayment(model.CustomerId.Value, model.TransactionCode, model.Amount, model.Comment);
            if (result != null)
                throw new ArgumentException(result);

            return RedirectToAction("Index", "Customer", new { customerID = model.CustomerId });
        }
        catch (Exception e)
        {
            return View(model).WithDanger(e.Message, "");
        }
    }
}
