using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.InternalWeb.Extensions;
using SW.InternalWeb.Models.Customer;

namespace SW.InternalWeb.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personEntityService;
    private readonly ITransactionService transactionService;
    private readonly IMieDataService mieDataService;

    public CustomerController(
        ICustomerService customerService,
        IPersonEntityService personEntityService,
        ITransactionService transactionService,
        IMieDataService mieDataService)
    {
        this.customerService = customerService;
        this.personEntityService = personEntityService;
        this.transactionService = transactionService;
        this.mieDataService = mieDataService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? customerID)
    {
        if (!customerID.HasValue)
            return View(new CustomerViewModel());

        var customer = await customerService.GetById(customerID.Value);
        if (customer == null)
            return View(new CustomerViewModel()).WithDanger("Customer not found", "");

        var personEntityTask = personEntityService.GetById(customer.Pe);
        var latestTransactionTask = transactionService.GetLatest(customer.CustomerId);
        var pastDueAmountTask = transactionService.GetPastDueAmount(customer.CustomerId);
        var pastDue30DaysAmountTask = transactionService.Get30DaysPastDueAmount(customer.CustomerId);
        var pastDue60DaysAmountTask = transactionService.Get60DaysPastDueAmount(customer.CustomerId);
        var pastDue90DaysAmountTask = transactionService.Get90DaysPastDueAmount(customer.CustomerId);
        var activeImagesTask = mieDataService.Get(customer.CustomerId.ToString(), true);
        var inactiveImagesTask = mieDataService.Get(customer.CustomerId.ToString(), false);

        var personEntity = await personEntityTask;
        var latestTransaction = await latestTransactionTask;
        var pastDueAmount = await pastDueAmountTask;
        var pastDue30DaysAmount = await pastDue30DaysAmountTask;
        var pastDue60DaysAmount = await pastDue60DaysAmountTask;
        var pastDue90DaysAmount = await pastDue90DaysAmountTask;
        var activeImages = await activeImagesTask;
        var inactiveImages = (await inactiveImagesTask).OrderByDescending(MieData => MieData.ChgDateTime).ToList();

        var customerType = Helpers.CustomerCodes.First(t => t.Value == customer.CustomerType).Text;

        CustomerViewModel vm = new()
        {
            CustomerType = customerType,
            CustomerID = customer.CustomerId,
            NameAttn = customer.NameAttn,
            Contact = customer.Contact,
            EffectiveDate = customer.EffectiveDate,
            CancelDate = customer.CancelDate,
            ContractCharge = customer.ContractCharge?.ToString("c"),
            PurchaseOrder = customer.PurchaseOrder,
            Notes = customer.Notes,
            CurrentBalance = latestTransaction?.TransactionBalance ?? 0.00m,
            FullName = personEntity.FullName,
            NameTypeFlag = personEntity.NameTypeFlag,
            PastDueAmount = pastDueAmount,
            PastDue30Days = pastDue30DaysAmount,
            PastDue60Days = pastDue60DaysAmount,
            PastDue90Days = pastDue90DaysAmount,
            CollectionsBalance = latestTransaction?.CollectionsBalance ?? 0.00m,
            CounselorsBalance = latestTransaction?.CounselorsBalance ?? 0.00m,
            UncollectableBalance = latestTransaction?.UncollectableBalance ?? 0.00m,
            LegacyCustomerID = customer.LegacyCustomerId,
            Account = "SW" + personEntity.Account,
            ActiveImages = activeImages,
            InactiveImages = inactiveImages
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Update(CustomerViewModel vm)
    {
        var activeImagesTask = mieDataService.Get(vm.CustomerID?.ToString(), true);
        var inactiveImagesTask = mieDataService.Get(vm.CustomerID?.ToString(), false);
        var customerTask = customerService.GetById(vm.CustomerID.Value);
        vm.ActiveImages = await activeImagesTask;
        vm.InactiveImages = (await inactiveImagesTask).OrderByDescending(MieData => MieData.ChgDateTime).ToList();
        var customer = await customerTask;

        if (!ModelState.IsValid)
            return View("Index", vm).WithDanger("There are errors on the form", "");

        bool customerCancel = false;
        if (customer.CancelDate != vm.CancelDate)
        {
            if (vm.CancelDate < DateTime.Today.Date)
            {
                return View("Index", vm)
                    .WithDanger("Cancel Date before " + DateTime.Today.Date.ToShortDateString(), "");
            }
            else
            {
                if (vm.CancelDate.HasValue)
                {
                    customerCancel = true;
                }
            }
        }

        while (vm.FullName.Contains("  ")) vm.FullName = vm.FullName.Replace("  ", " ");
        var personEntity = await personEntityService.GetById(customer.Pe);

        customer.ChgDateTime = DateTime.Now;
        customer.ChgToi = User.GetNameOrEmail();
        customer.EffectiveDate = vm.EffectiveDate.Value;
        customer.CancelDate = vm.CancelDate;
        customer.NameAttn = vm.NameAttn;
        customer.Contact = vm.Contact;
        customer.Notes = vm.Notes;
        customer.PurchaseOrder = vm.PurchaseOrder;

        personEntity.ChgDateTime = DateTime.Now;
        personEntity.ChgToi = User.GetNameOrEmail();
        personEntity.FullName = vm.FullName;

        await customerService.Update(customer);
        await personEntityService.Update(personEntity);
        if (customerCancel)
        {
            await customerService.CancelRelatedEntities(customer, User.GetNameOrEmail());
        }

        return View("Index", vm).WithSuccess("Customer updated", "");
    }

    public IActionResult Search(int CustomerID)
    {
        return RedirectToAction(nameof(Index), new { customerID = CustomerID });
    }
}
