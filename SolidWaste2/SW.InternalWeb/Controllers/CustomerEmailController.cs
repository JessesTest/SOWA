using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.InternalWeb.Models.CustomerEmail;

namespace SW.InternalWeb.Controllers;

public class CustomerEmailController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personEntityService;
    private readonly IEmailService emailService;

    public CustomerEmailController(
        ICustomerService customerService,
        IPersonEntityService personEntityService,
        IEmailService emailService)
    {
        this.customerService = customerService;
        this.personEntityService = personEntityService;
        this.emailService = emailService;
    }

    public async Task<IActionResult> Index(int customerId/*, int? id*/)
    {
        var customer = await customerService.GetById(customerId);
        if (customer == null || customer.DelDateTime != null)
            return RedirectToAction("Index", "Customer")
                .WithDanger("Customer not found", $"{customerId}");

        var person = await personEntityService.GetById(customer.Pe);
        if (person == null || person.Delete)
            return RedirectToAction("Index", "Customer", new { customerId })
                .WithDanger("Invalid customer", "customer has no person record");

        var emails = person.Emails
            .Where(e => e.IsDefault)
            .OrderBy(e => e.AddDateTime)
            .ToList();

        Email email = null;
        if (emails.Any())
        {
            //if (id != null)
            //    email = emails.FirstOrDefault(e => e.Id == id);

            email ??= emails.FirstOrDefault();
        }
        var currentIndex = email == null ? 0 : emails.IndexOf(email);

        var paperLess = person.PaperLess switch
        {
            null => 3,
            true => 1,
            false => 2
        };

        CustomerEmailViewModel vm = new()
        {
            CurrentIndex = currentIndex + 1,
            CustomerID = customer.CustomerId,
            CustomerType = customer.CustomerType,
            Email1 = email?.Email1,
            FullName = person.FullName,
            Id = email?.Id,
            MaxIndex = person.Emails.Count,
            PaperLess = paperLess,
            Status = email?.Status ?? true,
            Type = email?.Type ?? 7
        };

        return View(vm)
            .WithInfoWhen(customer.PaymentPlan, "", "Customer has a payment plan.");
    }

    [HttpPost]
    public async Task<IActionResult> Update(CustomerEmailViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm).WithWarning("", "There are errors on the form");
        }

        var email = await emailService.GetById(vm.Id.Value);
        if (email == null)
            return RedirectToAction("Index", "Customer")
                .WithWarning("Error", "Email not found");

        var customer = await customerService.GetById(vm.CustomerID);
        if (customer == null)
            return RedirectToAction("Index", "Customer")
                .WithWarning("Error", "Customer not found");

        var person = await personEntityService.GetById(customer.Pe);

        email.ChgDateTime = DateTime.Now;
        email.ChgToi = User.GetNameOrEmail();
        email.Email1 = vm.Email1.ToLower();
        email.Type = vm.Type;
        email.Status = vm.Status;
        await emailService.Update(email);

        bool? paperLess = vm.PaperLess switch
        {
            1 => true,
            2 => false,
            _ => null
        };
        if(person.PaperLess != paperLess)
        {
            person.ChgDateTime = DateTime.Now;
            person.ChgToi = User.GetNameOrEmail();
            person.PaperLess = paperLess;
            person.Addresses = null;
            person.Code = null;
            person.Emails = null;
            person.Phones = null;
            await personEntityService.Update(person);
        }

        return RedirectToAction(nameof(Index), new { vm.CustomerID, vm.Id })
            .WithSuccess("Success", "Email updated")
            .WithInfoWhen(customer.PaymentPlan, "", "Customer has a payment plan.");
    }

    [HttpPost]
    public async Task<IActionResult> Clear(CustomerEmailViewModel vm)
    {
        ModelState.Clear();
        vm.Id = null;
        vm.Email1 = string.Empty;

        var customer = await customerService.GetById(vm.CustomerID);
        return View(vm)
            .WithInfoWhen(customer?.PaymentPlan ?? false, "", "Customer has a payment plan.");
    }

    [HttpPost]
    public async Task<IActionResult> Add(CustomerEmailViewModel vm)
    {
        var customer = await customerService.GetById(vm.CustomerID);
        if (customer == null)
            return RedirectToAction("Index", "Customer")
                .WithWarning("Error", "Customer not found");

        if (!ModelState.IsValid)
            return View("Index", vm)
                .WithWarning("", "There were field validation errors")
                .WithInfoWhen(customer?.PaymentPlan ?? false, "", "Customer has a payment plan.");

        Email email = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            Email1 = vm.Email1?.ToLower(),
            IsDefault = true,
            PersonEntityID = customer.Pe,
            Status = vm.Status,
            Type = vm.Type
        };

        if (TryValidateModel(email))
        {
            await emailService.Add(email);
            return RedirectToAction("Index", new { customerID = vm.CustomerID, id = email.Id })
                .WithSuccess("Success", "Email added");
        }

        return View("Index", vm)
            .WithWarning("", "There were field validation errors")
            .WithInfoWhen(customer?.PaymentPlan ?? false, "", "Customer has a payment plan.");
    }

    public IActionResult Next(CustomerEmailViewModel vm)
    {
        return RedirectToAction("Index", new { vm.CustomerID, vm.Id });
    }

    public IActionResult Previous(CustomerEmailViewModel vm)
    {
        return RedirectToAction("Index", new { vm.CustomerID, vm.Id });
    }

}
