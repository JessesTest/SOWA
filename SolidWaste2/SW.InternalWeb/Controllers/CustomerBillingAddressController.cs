using Common.Services.AddressValidation;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Extensions;
using SW.InternalWeb.Models.CustomerBillingAddress;
namespace SW.InternalWeb.Controllers;

public class CustomerBillingAddressController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personEntityService;
    private readonly IAddressService addressService;
    private readonly IAddressValidationService addressValidationService;
    private readonly ICodeService codeService;

    public CustomerBillingAddressController(
        ICustomerService customerService,
        IPersonEntityService personEntityService,
        IAddressService addressService,
        IAddressValidationService addressValidationService,
        ICodeService codeService)
    {
        this.customerService = customerService;
        this.personEntityService = personEntityService;
        this.addressService = addressService;
        this.addressValidationService = addressValidationService;
        this.codeService = codeService;
    }

    #region Index

    [HttpGet]
    public async Task<IActionResult> Index(int customerId)
    {
        HttpContext.Session.Remove("CustomerBillingAddress.Addresses");

        var customer = await customerService.GetById(customerId);
        if (customer == null)
            return RedirectToAction("Index", "CustomerInquiry")
                .WithDanger("Customer not found", "");

        var personEntity = await personEntityService.GetById(customer.Pe);
        var addresses = await addressService.GetByPerson(personEntity.Id, false);
        var ad = addresses.SingleOrDefault(a => !a.Delete && a.Code.Code1 == "B" && a.IsDefault);

        CustomerBillingAddressViewModel model;
        if (ad != null)
        {
            model = new()
            {
                Id = ad.Id,
                Override = ad.Override,
                AddressLine1 = ad.FormatAddressLine1(),
                AddressLine2 = ad.FormatAddressLine2(),
                City = ad.City,
                State = ad.State,
                Zip = ad.Zip,
                CustomerId = customer.CustomerId,
                Undeliverable = personEntity.Pab
            };
        }
        else
        {
            model = new()
            {
                State = "KS",
                CustomerId = customer.CustomerId,
                Undeliverable = personEntity.Pab
            };
        }

        return View(model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "")
            .WithWarningWhen(personEntity.Pab == true, "Account has undeliverable address.", "");
    }

    [HttpPost]
    public async Task<IActionResult> Index(CustomerBillingAddressViewModel model)
    {
        var customer = await customerService.GetById(model.CustomerId);
        if (customer == null)
            return RedirectToAction("Index", "CustomerInquiry")
                .WithDanger("Customer not found", "");

        var personEntity = await personEntityService.GetById(customer.Pe);
        var updated = false;
        bool additionalInfo;

        if (!ModelState.IsValid)
            return View(model).WithDanger("There are field errors", "");
        else if (model.Override && model.Zip == null)
            return View(model).WithDanger("Zip code Required if address override is checked", "");
        else if (model.State.ToUpper() != "KS" && model.Zip == null)
            return View(model).WithDanger("Zip code Required if State is not KS", "");
        else
        {
            try
            {
                additionalInfo = await Process(model);
            }
            catch (Exception ex)
            {
                return View(model).WithDanger(ex.Message, "");
            }

            if (additionalInfo)
            {
                await Update(model, customer);
                updated = true;
            }
        }

        return View(model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "")
            .WithWarningWhen(personEntity.Pab == true, "Account has undeliverable address.", "")
            .WithInfoWhen(!additionalInfo, "Select an address from the list", "")
            .WithSuccessWhen(updated, "Billing address updated", "");
    }

    private async Task<bool> Process(CustomerBillingAddressViewModel model)
    {
        if (model.Override || (model.State != null && model.State.ToUpper() != "KS"))
        {
            return true;
        }
        if (model.City == null || model.City.Trim().Length == 0)
        {
            throw new ArgumentException("City is required.");
        }
        if (model.State == null || model.State.Trim().Length == 0)
        {
            throw new ArgumentException("State is required.");
        }

        ICollection<ValidAddress> temp;
        try
        {
            temp = await addressValidationService.GetCandidates(model.AddressLine1, model.City, model.Zip, 8);
        }
        catch (Exception)
        {
            throw new ArgumentException("Address not found.");
        }

        if (temp.Count == 0)
        {
            throw new ArgumentException("Address not found");
        }

        if (temp.Count == 1)
        {
            var a = temp.First();
            model.AddressLine1 = a.Address;
            //model.AddressLine2
            model.City = a.City;
            model.State = a.State;
            model.Zip = a.Zip;
            return true;
        }

        ModelState.Clear();

        model.Addresses = temp
            .Select(a => new ValidAddressDto
            {
                AddressLine1 = a.Address,
                AddressLine2 = model.AddressLine2,
                City = a.City,
                State = a.State,
                Zip = a.Zip
            })
            .ToList();

        HttpContext.Session.SetString("CustomerBillingAddress.Addresses", System.Text.Json.JsonSerializer.Serialize(model.Addresses));

        return false;
    }

    private async Task Update(CustomerBillingAddressViewModel model, Customer customer)
    {
        Address ad;
        if (model.Id > 0)
        {
            ad = await addressService.GetById(model.Id);
            ad.ChgDateTime = DateTime.Now;
            ad.ChgToi = User.Identity.Name;
        }
        else
        {
            var addressType = await codeService.Get("Address", "B");

            ad = new Address
            {
                AddDateTime = DateTime.Now,
                AddToi = User.Identity.Name,
                PersonEntityID = customer.Pe,
                Type = addressType.Id,
            };
        }
        ad.Number = null;
        ad.Direction = null;
        ad.StreetName = model.AddressLine1;
        ad.Suffix = null;
        ad.Apt = model.AddressLine2;
        ad.City = model.City;
        ad.State = model.State;
        ad.Zip = model.Zip;

        if (model.Id > 0)
            await addressService.Update(ad);
        else
            await addressService.Add(ad);

        HttpContext.Session.Remove("CustomerBillingAddress.Addresses");
        ModelState.Clear();
    }

    #endregion

    [HttpPost]
    public async Task<IActionResult> SelectBillingAddress(CustomerBillingAddressViewModel model)
    {
        var customer = await customerService.GetById(model.CustomerId);
        if (customer == null || customer.DelDateTime != null)
            return RedirectToAction("Index", "Home").WithDanger("Customer not found", "");

        var json = HttpContext.Session.GetString("CustomerBillingAddress.Addresses");
        var addresses = System.Text.Json.JsonSerializer.Deserialize<ValidAddressDto[]>(json);
        var address = addresses[model.SelectIndex];

        var cbavm = new CustomerBillingAddressViewModel
        {
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            Zip = address.Zip,
            CustomerId = model.CustomerId,
            Id = model.Id
            //Override = false,
            //SelectIndex = 0,
            //Undeliverable = model.Undeliverable
        };

        await Update(cbavm, customer);

        return RedirectToAction(nameof(Index), new { customerId = customer.CustomerId })
            .WithSuccess("Billing address updated", "");
    }
}
