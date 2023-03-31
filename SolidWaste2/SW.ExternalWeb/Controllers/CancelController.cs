using Common.Extensions;
using Common.Services.Email;
using Common.Web.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.DM;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.Cancel;

namespace SW.ExternalWeb.Controllers;

[Authorize]
public class CancelController : Controller
{
    private readonly IAddressService addressService;
    private readonly IContainerService containerService;
    private readonly ICustomerService customerService;
    private readonly IEmailService peEmailService;
    private readonly IPersonEntityService personEntityService;
    private readonly IPhoneService phoneService;
    private readonly ISendGridService sendGridService;
    private readonly IServiceAddressService serviceAddressService;
    private readonly UserManager<ApplicationUser> userManager;

    private readonly string StaffEmail = "asim.shaikh@snco.us";

    public CancelController(
        IAddressService addressService,
        IContainerService containerService,
        ICustomerService customerService,
        IEmailService peEmailService,
        IPersonEntityService personEntityService,
        IPhoneService phoneService,
        ISendGridService sendGridService,
        IServiceAddressService serviceAddressService,
        UserManager<ApplicationUser> userManager)
    {
        this.addressService = addressService;
        this.containerService = containerService;
        this.customerService = customerService;
        this.peEmailService = peEmailService;
        this.personEntityService = personEntityService;
        this.phoneService = phoneService;
        this.sendGridService = sendGridService;
        this.serviceAddressService = serviceAddressService;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.FindByIdAsync(User.GetUserId());
        var person = await personEntityService.GetById(user.UserId);
        var phones = await phoneService.GetByPerson(person.Id, false);
        var phone = phones.FirstOrDefault(p => p.IsDefault) ?? phones.FirstOrDefault();
        var addresses = await addressService.GetByPerson(person.Id, false);
        var billingAddress = addresses.FirstOrDefault(a => a.Code.Code1 == "B");
        var customer = await customerService.GetByPE(person.Id)
            ?? throw new InvalidOperationException("Customer record not found");

        CancelViewModel model = new()
        {
            AccountNumber = string.Format("{0:000000}", customer.CustomerId),
            BillingAddress1 = string.Format("{0} {1} {2} {3} {4}", billingAddress.Number, billingAddress.Direction, billingAddress.StreetName, billingAddress.Suffix, billingAddress.Apt),
            BillingAddress2 = string.Format("{0} {1} {2}", billingAddress.City, billingAddress.State, billingAddress.Zip),
            CancelDate = null,
            ContainerId = null,
            Phone = phone?.PhoneNumber,
            ServiceAddressId = null
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(CancelViewModel model)
    {
        if(!ModelState.IsValid)
            return View(model);

        if (model.CancelDate.Value.DayOfWeek == System.DayOfWeek.Sunday)
            return View(model).WithDanger("Cancel date may not be a Sunday", "");

        var user = await userManager.FindByIdAsync(User.GetUserId());
        var person = await personEntityService.GetById(user.UserId);
        var customer = await customerService.GetByPE(person.Id);
        var serviceAddress = await serviceAddressService.GetById(model.ServiceAddressId.Value);

        if (customer.CustomerId != serviceAddress.CustomerId)
            throw new InvalidOperationException("Invalid customer");

        if (model.ContainerId.Value > 0)
        {
            var container = await containerService.GetById(model.ContainerId.Value);
            if (container.ServiceAddressId != model.ServiceAddressId)
                throw new InvalidOperationException("Invalid service address");

            await containerService.CustomerCancelContainer(model.CancelDate.Value, person, model.ContainerId.Value, User.Identity.Name);
            await CancelContainerEmail(model.ContainerId.Value, person, customer);
        }
        else
        {
            await serviceAddressService.CustomerCancelServiceAddress(model.CancelDate.Value, person, model.ServiceAddressId.Value, User.Identity.Name);
            await CancelAddressEmail(model.ServiceAddressId.Value, person, customer);
        }

        return RedirectToAction("Index", "Home", null);
    }

    public async Task<IActionResult> ServiceAddresses()
    {
        var user = await userManager.FindByIdAsync(User.GetUserId());
        var person = await personEntityService.GetById(user.UserId);
        var customer = await customerService.GetByPE(person.Id);
        var serviceAddresses = await serviceAddressService.GetByCustomer(customer.CustomerId);

        foreach (var sa in serviceAddresses)
        {
            sa.PEAddress = person.Addresses.FirstOrDefault(a => a.Id == sa.PeaddressId);
        }

        var model = serviceAddresses
            .Where(a => !a.CancelDate.HasValue || a.CancelDate.Value > DateTime.Now)
            .ToList();

        return PartialView(model);
    }

    public async Task<IActionResult> Containers(int serviceAddressId)
    {
        var user = await userManager.FindByIdAsync(User.GetUserId());
        var person = await personEntityService.GetById(user.UserId);
        var customer = await customerService.GetByPE(person.Id);
        var serviceAddress = await serviceAddressService.GetById(serviceAddressId);

        if (serviceAddress == null || serviceAddress.CustomerId != customer.CustomerId)
            throw new InvalidOperationException("Invalid service address");

        var containers = (await containerService.GetByServiceAddress(serviceAddress.Id))
            .Where(c => !c.CancelDate.HasValue || c.CancelDate.Value > DateTime.Now)
            .ToList();
        return PartialView(containers);
    }

    [NonAction]
    private async Task CancelContainerEmail(int containerId, PersonEntity person, Customer customer)
    {
        var container = await containerService.GetById(containerId, customer.CustomerId);

        var emails = await peEmailService.GetByPerson(person.Id, false);
        var email = emails.FirstOrDefault(e => !e.Delete && e.IsDefault)
            ?? person.Emails.FirstOrDefault(e => !e.Delete);
        
        var serviceAddress = container?.ServiceAddress;
        if(serviceAddress != null)
        {
            serviceAddress.PEAddress = await addressService.GetById(serviceAddress.PeaddressId);
        }

        var peAddresses = await addressService.GetByPerson(person.Id, false);
        var billingAddress = peAddresses.FirstOrDefault(a => a.Code.Code1 == "B");

        var model = new CancelEmailViewModel
        {
            Address = billingAddress,
            Containers = new[] { container },
            Customer = customer,
            Email = email,
            Person = person,
            Phone = person.Phones.FirstOrDefault(p => p.IsDefault),
            ServiceAddress = serviceAddress
        };

        var customerHtml = await this.RenderViewAsync("CancelContainerEmailForCustomer", model);
        SendEmailDto customerEmail = new()
        {
            Subject = "SW: Cancel Container",
            HtmlContent = customerHtml
        };
        customerEmail.AddTo(email.Email1);

        var staffHtml = await this.RenderViewAsync("CancelContainerEmailForStaff", model);
        SendEmailDto staffEmail = new()
        {
            Subject = "SW: Cancel Container",
            HtmlContent = staffHtml
        };
        staffEmail.AddTo(StaffEmail);

        _ = sendGridService.SendSingleEmail(customerEmail);
        _ = sendGridService.SendSingleEmail(staffEmail);
    }

    [NonAction]
    private async Task CancelAddressEmail(int serviceAddressId, PersonEntity person, Customer customer)
    {
        var peAddresses = await addressService.GetByPerson(person.Id, false);
        var billingAddress = peAddresses.FirstOrDefault(a => a.Code.Code1 == "B");

        var emails = await peEmailService.GetByPerson(person.Id, false);
        var email = emails.FirstOrDefault(e => !e.Delete && e.IsDefault)
            ?? person.Emails.FirstOrDefault(e => !e.Delete);

        var phone = person.Phones.First(p => !p.Delete && p.IsDefault);
        
        var serviceAddress = await serviceAddressService.GetById(serviceAddressId);
        serviceAddress.PEAddress = await addressService.GetById(serviceAddress.PeaddressId);

        var containers = (await containerService.GetByServiceAddress(serviceAddress.Id))
            .Where(c => !c.DeleteFlag && c.CancelDate.Value == serviceAddress.CancelDate.Value)
            .ToList();

        var model = new CancelEmailViewModel
        {
            Address = billingAddress,
            Customer = customer,
            Email = email,
            Person = person,
            Phone = phone,
            ServiceAddress = serviceAddress,
            Containers = containers
        };

        var customerEmail = new SendEmailDto
        {
            Subject = "SW: Cancel Service Address",
            HtmlContent = await this.RenderViewAsync("CancelAddressEmailForCustomer", model),
        }
        .AddTo(email.Email1);

        var staffEmail = new SendEmailDto
        {
            Subject = "SW: Cancel Service Address",
            HtmlContent = await this.RenderViewAsync("CancelAddressEmailForStaff", model),
        }
        .AddTo(StaffEmail);

        _ = sendGridService.SendSingleEmail(customerEmail);
        _ = sendGridService.SendSingleEmail(staffEmail);
    }
}
