using Common.Services.AddressValidation;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Extensions;
using SW.InternalWeb.Models.CustomerBillingAddress;
using Twilio.Rest.Autopilot.V1.Assistant;
using static Microsoft.Graph.CoreConstants;

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
        if (customer == null || customer.DelDateTime != null)
            return NotFound();

        var personEntity = await personEntityService.GetById(customer.Pe);
        if (personEntity.Pab == true)
        {
            ModelState.AddModelError("warning", "Account has undeliverable address.");
        }

        var addresses = await addressService.GetByPerson(personEntity.Id, false);
        var ad = addresses.SingleOrDefault(a => !a.Delete && a.Code.Code1 == "B" && a.IsDefault);

        CustomerBillingAddressViewModel model;
        if (ad != null)
        {
            model = new()
            {
                //Addresses
                AddressLine1 = ad.FormatAddress(),
                Apt = ad.Apt,
                City = ad.City,
                //CustomerID 
                //CustomerType
                Direction = ad.Direction,
                Id = ad.Id,
                Number = ad.Number?.ToString(),
                Override = ad.Override,
                //SelectIndex
                State = ad.State,
                StreetName = ad.StreetName,
                Suffix = ad.Suffix,
                //Undeliverable
                Zip = ad.Zip
            };
        }
        else
        {
            model = new()
            {
                State = "KS"
            };
        }
        model.CustomerID = customer.CustomerId;
        model.CustomerType = customer.CustomerType;
        model.Undeliverable = personEntity.Pab;

        if (customer.PaymentPlan)
            ModelState.AddModelError("info", "Customer has a payment plan.");

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(model));

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(CustomerBillingAddressViewModel model)
    {
        var customer = await customerService.GetById(model.CustomerID);
        if (customer.PaymentPlan)
            ModelState.AddModelError("info", "Customer has a payment plan.");

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("exception", "There are field errors");
        }
        else if (model.Override && model.Zip == null)
        {
            ModelState.AddModelError("exception", "Zip code Required if address override is checked");
        }
        else if (model.State.ToUpper() != "KS" && model.Zip == null)
        {
            ModelState.AddModelError("exception", "Zip code Required if State is not KS");
        }
        else if (await Process(model))
        {
            await Update(model, customer);
        }

        return View(model);
    }

    private async Task<bool> Process(CustomerBillingAddressViewModel model)
    {
        if (model.Override || (model.State != null && model.State.ToUpper() != "KS"))
        {
            model.Number = "";
            model.Direction = "";
            model.StreetName = model.AddressLine1;
            model.Suffix = "";
            model.Apt = "";
            return true;
        }
        if (!model.Override)
        {
            if (model.City == null || model.City.Trim().Length == 0)
            {
                ModelState.AddModelError("exception", "City is required.");
                return false;
            }
            if (model.State == null || model.State.Trim().Length == 0)
            {
                ModelState.AddModelError("exception", "State is required.");
                return false;
            }
        }

        ICollection<ValidAddress> temp;
        try
        {
            temp = await addressValidationService.GetCandidates(model.AddressLine1, model.City, model.Zip, 2);
        }
        catch (Exception)
        {
            ModelState.AddModelError("exception", "Address not found.");
            return false;
        }

        if (temp.Count == 0)
        {
            ModelState.AddModelError("exception", "Address not found");
            return false;
        }

        if (temp.Count == 1)
        {
            var a = temp.First();
            model.Number = "";
            model.Direction = "";
            model.StreetName = a.Address;
            model.Suffix = "";
            model.Apt = "";
            model.City = a.City;
            model.State = a.State;
            model.Zip = a.Zip;
            return true;
        }

        ModelState.Clear();

        model.Addresses = temp
            .Select(a => new CustomerBillingAddressViewModel
            {
                Number = "",
                Direction = "",
                StreetName = a.Address,
                Suffix = "",
                Apt = "",
                City = a.City,
                State = a.State,
                Zip = a.Zip
            })
            .ToList();

        HttpContext.Session.SetString("CustomerBillingAddress.Addresses", System.Text.Json.JsonSerializer.Serialize(model.Addresses));

        ModelState.AddModelError("info", "Select an address from the list");
        return false;
    }

    private async Task Update(CustomerBillingAddressViewModel model, Customer customer)
    {
        Address ad;
        if (model.Id.HasValue && model.Id.Value > 0)
        {
            ad = await addressService.GetById(model.Id.Value);
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
        ad.Number = int.TryParse(model.Number, out var parsedInt) ? parsedInt : null;
        ad.Direction = model.Direction;
        ad.StreetName = model.StreetName;
        ad.Suffix = model.Suffix;
        ad.Apt = model.Apt;
        ad.City = model.City;
        ad.State = model.State;
        ad.Zip = model.Zip;

        if (model.Id.HasValue)
            await addressService.Update(ad);
        else
            await addressService.Add(ad);

        HttpContext.Session.Remove("CustomerBillingAddress.Addresses");
        ModelState.Clear();
        model.AddressLine1 = ad.FormatAddress();
        ModelState.AddModelError("success", "Address updated");
    }
    #endregion


    [HttpPost]
    public async Task<IActionResult> SelectBillingAddress(CustomerBillingAddressViewModel model)
    {
        var customer = await customerService.GetById(model.CustomerID);
        if (customer == null || customer.DelDateTime != null)
            return NotFound();

        var json = HttpContext.Session.GetString("CustomerBillingAddress.Addresses");
        var addresses = System.Text.Json.JsonSerializer.Deserialize<CustomerBillingAddressViewModel[]>(json);
        var address = addresses[model.SelectIndex];
        
        await Update(address, customer);
        if (customer.PaymentPlan)
            ModelState.AddModelError("info", "Customer has a payment plan.");

        return View("Index", address);
    }
}
