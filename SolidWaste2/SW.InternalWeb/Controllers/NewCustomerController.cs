using Common.Extensions;
using Common.Services.AddressValidation;
using Common.Services.Email;
using Common.Web.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Extensions;
using SW.InternalWeb.Models.NewCustomer;
using System.Text;
using System.Text.Json;

namespace SW.InternalWeb.Controllers;

public class NewCustomerController : Controller
{
    private readonly IAddressValidationService addressValidationService;
    private readonly IContainerRateService rateService;
    private readonly IContainerCodeService containerCodeService;
    private readonly IRefuseRouteService routeService;
    private readonly IPersonEntityService personService;
    private readonly ICodeService codeService;
    private readonly IAddressService addressService;
    private readonly ICustomerService customerService;
    private readonly IPhoneService phoneService;
    private readonly IEmailService emailService;
    private readonly IServiceAddressService serviceAddressService;
    private readonly IContainerService containerService;
    private readonly IServiceAddressNoteService noteService;
    private readonly ISendGridService sendGridService;

    public NewCustomerController(
        IAddressValidationService addressValidationService,
        IContainerRateService rateService,
        IContainerCodeService containerCodeService,
        IRefuseRouteService routeService,
        IPersonEntityService personService,
        ICodeService codeService,
        IAddressService addressService,
        ICustomerService customerService,
        IPhoneService phoneService,
        IEmailService emailService,
        IServiceAddressService serviceAddressService,
        IContainerService containerService,
        IServiceAddressNoteService noteService,
        ISendGridService sendGridService)
    {
        this.addressValidationService = addressValidationService;
        this.rateService = rateService;
        this.containerCodeService = containerCodeService;
        this.routeService = routeService;
        this.personService = personService;
        this.codeService = codeService;
        this.addressService = addressService;
        this.customerService = customerService;
        this.phoneService = phoneService;
        this.emailService = emailService;
        this.serviceAddressService = serviceAddressService;
        this.containerService = containerService;
        this.noteService = noteService;
        this.sendGridService = sendGridService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(CustomerInformation));
    }

    #region Customer

    [HttpGet]
    public IActionResult CustomerInformation()
    {
        var model = GetCustomer();
        return View(model);
    }

    [HttpPost]
    public IActionResult CustomerInformation(CustomerViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model).WithDanger("Invalid customer code", "");

        while (model.FullName.Contains("  ")) model.FullName = model.FullName.Replace("  ", " ");

        SetCustomer(model);

        return RedirectToAction(nameof(CustomerBillingAddress));
    }

    [HttpPost]
    public IActionResult CustomerInformation_Clear(CustomerViewModel model)
    {
        RemoveCustomer();
        RemoveBillingAddress();
        RemoveValidBillingAddresses();
        RemovePhone();
        RemoveEmail();
        RemoveServiceAddressList();
        RemoveValidServiceAddresses();
        ModelState.Clear();

        return RedirectToAction(nameof(CustomerInformation));
    }

    private CustomerViewModel GetCustomer()
    {
        return Get<CustomerViewModel>("CustomerInfo") ??
            new CustomerViewModel { EffectiveDate = DateTime.Today };
    }
    private void SetCustomer(CustomerViewModel model)
    {
        Set("CustomerInfo", model);
    }
    private void RemoveCustomer()
    {
        Remove("CustomerInfo");
    }

    #endregion

    #region Billing Address

    public IActionResult CustomerBillingAddress()
    {
        RemoveValidBillingAddresses();
        var model = GetBillingAddress() ?? new BillingAddressViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CustomerBillingAddress(BillingAddressViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model).WithDanger("Invalid Customer Billing Address 1", "");

        var old = GetBillingAddress();
        model.ApprovedAddress = old?.ApprovedAddress;

        RemoveValidBillingAddresses();

        try
        {
            await Process(model);
        }
        catch(Exception ex)
        {
            return View(model).WithDanger(ex.Message, "");
        }

        if (model.Addresses != null)
            return View(model).WithInfo("", "Select an adddress from the list");

        model.AddressLine1 = model.AddressLine1?.ToUpper();
        model.AddressLine2 = model.AddressLine2?.ToUpper();
        model.City = model.City?.ToUpper();
        model.State = model.State?.ToUpper();
        model.Zip = model.Zip?.ToUpper();

        model.SetApproved();

        SetBillingAddress(model);
        return RedirectToAction(nameof(CustomerPhoneNumber));
    }

    [HttpPost]
    public IActionResult CustomerBillingAddress_Prev(BillingAddressViewModel model)
    {
        if (ModelState.IsValid)
        {
            RemoveValidBillingAddresses();
            SetBillingAddress(model);
            return RedirectToAction(nameof(CustomerInformation));
        }
        return View(nameof(CustomerBillingAddress), model).WithDanger("Invalid Customer Billing Address 2", "");
    }

    // address verification
    public async Task Process(BillingAddressViewModel model)
    {
        model.AddressLine1 = model.AddressLine1?.ToUpper();
        model.AddressLine2 = model.AddressLine2?.ToUpper();
        model.City = model.City?.ToUpper();
        model.State = model.State?.ToUpper();
        model.Zip = model.Zip?.ToUpper();

        if (SkipAddressValidation(model))
        {
            if (string.IsNullOrWhiteSpace(model.City))
            {
                throw new ArgumentException("There are errors on the form");
            }
            if (string.IsNullOrWhiteSpace(model.State))
            {
                throw new ArgumentException("There are errors on the form");
            }
            if (string.IsNullOrWhiteSpace(model.Zip))
            {
                throw new ArgumentException("There are errors on the form");
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(model.City))
        {
            throw new ArgumentException("City is required.");
        }
        if (string.IsNullOrWhiteSpace(model.State))
        {
            throw new ArgumentException("State is required.");
        }

        var temp = await addressValidationService.GetCandidates(model.AddressLine1, model.City, model.Zip, 8);
        if (!temp.Any())
            throw new ArgumentException("Address not found");

        if (temp.Count == 1)
        {
            var a = temp[0];
            model.AddressLine1 = a.Address?.ToUpper();
            //model.AddressLine2 = model.AddressLine2
            model.City = a.City?.ToUpper();
            model.State = a.State?.ToUpper();
            model.Zip = a.Zip?.ToUpper();

            model.Addresses = null;
            return;
        }

        model.Addresses = temp.Select(a => new ValidAddressDto
        {
            AddressLine1 = a.Address?.ToUpper(),
            AddressLine2 = model.AddressLine2,
            City = a.City?.ToUpper(),
            State = a.State?.ToUpper(),
            Zip = a.Zip?.ToUpper()
        }).ToList();
        SetValidBillAddresses(model.Addresses);
    }

    private static bool SkipAddressValidation(BillingAddressViewModel model)
    {
        return model == null
            || model.Override
            || model.State != "KS"
            || model.AddressLine1.StartsWith("PO BOX")
            || model.AddressLine1.StartsWith("P.O BOX")
            || model.AddressLine1.StartsWith("P.O. BOX")
            || model.AddressLine1.StartsWith("P.O.BOX")
            || model.IsApproved();
    }

    [HttpPost]
    public IActionResult SelectBillingAddress(BillingAddressViewModel model)
    {
        var addresses = GetValidBillingAddresses();

        var address = addresses[model.SelectIndex];
        model.AddressLine1 = address.AddressLine1;
        model.AddressLine2 = address.AddressLine2;
        model.City = address.City;
        model.State = address.State;
        model.Zip = address.Zip;
        //model.Override = true
        model.SetApproved();

        RemoveValidBillingAddresses();
        SetBillingAddress(model);
        return RedirectToAction(nameof(CustomerPhoneNumber));
    }


    private BillingAddressViewModel GetBillingAddress()
    {
        return Get<BillingAddressViewModel>("CustomerBillingAddress") ??
            new BillingAddressViewModel { State = "KS" };
    }
    private void SetBillingAddress(BillingAddressViewModel model)
    {
        Set("CustomerBillingAddress", model);
    }
    private void RemoveBillingAddress()
    {
        Remove("CustomerBillingAddress");
    }

    private ValidAddressDto[] GetValidBillingAddresses()
    {
        return Get<ValidAddressDto[]>("NewCustomerBillingAddress.Addresses");
    }
    private void SetValidBillAddresses(IEnumerable<ValidAddressDto> model)
    {
        Set("NewCustomerBillingAddress.Addresses", model);
    }
    private void RemoveValidBillingAddresses()
    {
        Remove("NewCustomerBillingAddress.Addresses");
    }

    #endregion

    #region Phone

    public IActionResult CustomerPhoneNumber()
    {
        var model = GetPhone();
        if(model == null)
        {
            model = new PhoneNumberViewModel();
            SetPhone(model);
        }
        return View(model);
    }

    [HttpPost]
    public IActionResult CustomerPhoneNumber(PhoneNumberViewModel model)
    {
        if (ModelState.IsValid)
        {
            SetPhone(model);
            return RedirectToAction(nameof(CustomerEmail));
        }
        return View(model).WithDanger("The Phone Number is invalid", "");
    }

    [HttpPost]
    public IActionResult CustomerPhoneNumber_Prev(PhoneNumberViewModel model)
    {
        if (ModelState.IsValid)
            return RedirectToAction(nameof(CustomerBillingAddress));

        return View("CustomerPhoneNumber", model).WithDanger("The Phone Number is invalid", "");
    }

    private PhoneNumberViewModel GetPhone()
    {
        return Get<PhoneNumberViewModel>("CustomerPhoneNumber") ??
            new PhoneNumberViewModel();
    }
    private void SetPhone(PhoneNumberViewModel model)
    {
        Set("CustomerPhoneNumber", model);
    }
    private void RemovePhone()
    {
        Remove("CustomerPhoneNumber");
    }

    #endregion

    #region Email

    public IActionResult CustomerEmail()
    {
        var model = GetEmail();
        if(model == null)
        {
            model = new EmailViewModel();
            SetEmail(model);
        }
        return View(model);
    }

    [HttpPost]
    public IActionResult CustomerEmail(EmailViewModel model)
    {
        if (ModelState.IsValid)
        {
            SetEmail(model);
            return RedirectToAction(nameof(ServiceAddress));
        }
        return View(model).WithDanger("The Email Address is invalid", "");
    }

    [HttpPost]
    public IActionResult CustomerEmail_Prev(EmailViewModel model)
    {
        if (ModelState.IsValid)
            return RedirectToAction(nameof(CustomerPhoneNumber));

        return View("CustomerEmail", model).WithDanger("The Email Address is invalid", "");
    }

    private EmailViewModel GetEmail()
    {
        return Get<EmailViewModel>("CustomerEmail")
            ?? new EmailViewModel();
    }
    private void SetEmail(EmailViewModel model)
    {
        Set("CustomerEmail", model);
    }
    private void RemoveEmail()
    {
        Remove("CustomerEmail");
    }

    #endregion

    #region Service Address

    public IActionResult ServiceAddress()
    {
        var serviceAddresses = GetServiceAddressList();
        if(ServiceAddress == null)
        {
            serviceAddresses = new ServiceAddressList();
            SetServiceAddressList(serviceAddresses);
        }

        if (serviceAddresses.Any())
        {
            return View("ServiceAddressSummary", serviceAddresses);
        }

        //return RedirectToAction(nameof(AddAddress))
        return AddAddress();
    }

    public IActionResult AddAddress()
    {
        RemoveValidServiceAddresses();

        var customer = GetCustomer();

        ServiceAddressViewModel serviceAddress = new()
        {
            EffectiveDate = (new[] { DateTime.Today, customer.EffectiveDate.Value }).Max(),
            Id = Guid.NewGuid(),
            State = "KS"
        };
        return View("ServiceAddress", serviceAddress);
    }

    public IActionResult EditAddress(Guid id)
    {
        var list = GetServiceAddressList();

        var address = list.FirstOrDefault(m => m.Id == id);
        if (address == null)
            return RedirectToAction(nameof(ServiceAddress))
                .WithDanger("Address not found", "");

        return View("ServiceAddress", address);
    }

    public IActionResult RemoveAddress(Guid id)
    {
        var list = GetServiceAddressList();

        var item = list.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            list.Remove(item);
            SetServiceAddressList(list);
        }

        return RedirectToAction(nameof(ServiceAddress))
            .WithSuccess("Address removed", "");
    }

    public IActionResult SelectAddress(ServiceAddressViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(ServiceAddress))
                .WithDanger("Invalid Customer Service Address 1", "");

        var serviceAddressList = GetServiceAddressList();
        var validAddressList = GetValidServiceAddresses();

        var customer = GetCustomer();
        if (customer.EffectiveDate > model.EffectiveDate)
        {
            model.Addresses = validAddressList;
            return View("ServiceAddress", model).WithDanger("Service address effective date before customer effective date", "");
        }

        var serviceAddress = serviceAddressList.FirstOrDefault(a => a.Id == model.Id);
        if (serviceAddress == null)
        {
            serviceAddress = new()
            {
                Id = model.Id,
                EffectiveDate = model.EffectiveDate,
                Email = model.Email?.ToUpper(),
                LocationContact = model.LocationContact?.ToUpper(),
                LocationName = model.LocationName?.ToUpper(),
                Override = model.Override, // ?
                Phone = model.Phone?.ToUpper()
            };
            serviceAddressList.Add(serviceAddress);
        }
        var validAddress = validAddressList[model.AddressesIndex];

        serviceAddress.AddressLine1 = validAddress.AddressLine1;
        serviceAddress.AddressLine2 = validAddress.AddressLine2;
        serviceAddress.City = validAddress.City;
        serviceAddress.State = validAddress.State;
        serviceAddress.Zip = validAddress.Zip;

        RemoveValidServiceAddresses();
        SetServiceAddressList(serviceAddressList);

        return (serviceAddress.Containers.Any() ?
            RedirectToAction(nameof(serviceAddress)) :
            RedirectToAction(nameof(AddContainer), new { id = serviceAddress.Id }))
            .WithSuccess("Service address added", "");
    }

    public async Task<IActionResult> SaveAddress(ServiceAddressViewModel model)
    {
        RemoveValidServiceAddresses();

        if (!ModelState.IsValid)
            return View("ServiceAddress", model)
                .WithDanger("Invalid Customer Service Address 2", "");

        var customer = GetCustomer();
        if (customer.EffectiveDate > model.EffectiveDate)
            return View("ServiceAddress", model).WithDanger("Service address effective date before customer effective date", "");

        var serviceAddressList = GetServiceAddressList();
        var serviceAddress = serviceAddressList.FirstOrDefault(a => a.Id == model.Id);
        if(serviceAddress == null)
        {
            serviceAddress = new()
            {
                Id = model.Id
            };
            serviceAddressList.Add(serviceAddress);
        }

        serviceAddress.EffectiveDate = model.EffectiveDate;
        serviceAddress.Email = model.Email?.ToUpper();
        serviceAddress.LocationContact = model.LocationContact?.ToUpper();
        serviceAddress.LocationName = model.LocationName?.ToUpper();
        serviceAddress.Override = model.Override;
        serviceAddress.Phone = model.Phone?.ToUpper();

        if (SkipAddressValidation(model))
        {
            serviceAddress.AddressLine1 = model.AddressLine1?.ToUpper();
            serviceAddress.AddressLine2 = model.AddressLine2?.ToUpper();
            serviceAddress.City = model.City?.ToUpper();
            serviceAddress.State = model.State?.ToUpper();
            serviceAddress.Zip = model.Zip?.ToUpper();

            SetServiceAddressList(serviceAddressList);
            return (serviceAddress.Containers.Any() ?
                RedirectToAction(nameof(ServiceAddress)) :
                RedirectToAction(nameof(AddContainer), new { id = serviceAddress.Id }))
                .WithSuccess("Service address saved", "");
        }

        var validAddresses = await addressValidationService.GetCandidates(model.AddressLine1, model.City, model.Zip, 8);
        if (validAddresses.Count == 0)
            return View("ServiceAddress", model).WithDanger("Address not found", "");
        if(validAddresses.Count == 1)
        {
            var validAddress = validAddresses[0];
            serviceAddress.AddressLine1 = validAddress.Address?.ToUpper();
            serviceAddress.AddressLine2 = model.AddressLine2?.ToUpper();
            serviceAddress.City = validAddress.City?.ToUpper();
            serviceAddress.State = validAddress.State?.ToUpper();
            serviceAddress.Zip = validAddress.Zip?.ToUpper();

            SetServiceAddressList(serviceAddressList);

            return (serviceAddress.Containers.Any() ?
                RedirectToAction(nameof(ServiceAddress)) :
                RedirectToAction(nameof(AddContainer), new { id = serviceAddress.Id }))
                .WithSuccess("Service address saved", "");
        }

        var validServiceAddresses = validAddresses.Select(a => new ValidAddressDto
        {
            AddressLine1 = a.Address,
            AddressLine2 = model.AddressLine2,
            City = a.City,
            State = a.State,
            Zip = a.Zip
        }).ToList();

        SetServiceAddressList(serviceAddressList);
        SetValidServiceAddresses(validServiceAddresses);

        model.Addresses = validServiceAddresses;
        return View("ServiceAddress", model)
            .WithInfo("Select an address from the list", "");
    }

    private static bool SkipAddressValidation(ServiceAddressViewModel model)
    {
        return model == null
            || model.Override
            || model.State != "KS"
            || model.AddressLine1.StartsWith("PO BOX")
            || model.AddressLine1.StartsWith("P.O BOX")
            || model.AddressLine1.StartsWith("P.O. BOX")
            || model.AddressLine1.StartsWith("P.O.BOX");
    }

    public IActionResult ServiceAddressSummary2()
    {
        var list = GetServiceAddressList();
        return PartialView(list);
    }

    private ServiceAddressList GetServiceAddressList()
    {
        return Get<ServiceAddressList>("NewCustomerServiceAddresses") ??
            new ServiceAddressList();
    }
    private void SetServiceAddressList(ServiceAddressList list)
    {
        Set("NewCustomerServiceAddresses", list);
    }
    private void RemoveServiceAddressList()
    {
        Remove("NewCustomerServiceAddresses");
    }

    private ValidAddressDto[] GetValidServiceAddresses()
    {
        return Get<ValidAddressDto[]>("NewCustomerServiceAddress.Addresses")
            ?? Array.Empty<ValidAddressDto>();
    }
    private void SetValidServiceAddresses(IEnumerable<ValidAddressDto> list)
    {
        Set("NewCustomerServiceAddress.Addresses", list);
    }
    private void RemoveValidServiceAddresses()
    {
        Remove("NewCustomerServiceAddress.Addresses");
    }

    public IActionResult BillingAddress()
    {
        var billingAddressModel = GetBillingAddress();
        return Json(new
        {
            billingAddressModel.Override,
            billingAddressModel.AddressLine1,
            billingAddressModel.AddressLine2,
            billingAddressModel.City,
            billingAddressModel.State,
            billingAddressModel.Zip
        });
    }

    public IActionResult BillingEmail()
    {
        var emailModel = GetEmail();
        return Json(new { emailModel.Email });
    }

    public IActionResult BillingPhone()
    {
        var phoneModel = GetPhone();
        return Json(new { Phone = phoneModel.PhoneNumber });
    }

    #endregion

    #region Container

    public IActionResult AddContainer(Guid id)
    {
        var serviceAddresses = GetServiceAddressList();
        var serviceAddress = serviceAddresses.FirstOrDefault(a => a.Id == id);
        if (serviceAddress == null)
            return RedirectToAction(nameof(serviceAddress))
                .WithDanger("Service address not found", "");

        ContainerViewModel model = new()
        {
            Id = Guid.NewGuid(),
            ServiceAddressId = id,
            EffectiveDate = (new[] { DateTime.Today, serviceAddress.EffectiveDate }).Max()
        };
        return View("Container", model);
    }

    public IActionResult EditContaier(Guid id)
    {
        var list = GetServiceAddressList();
        var container = GetContainer(list, id);
        if(container == null)
            return RedirectToAction(nameof(ServiceAddress))
                .WithDanger("Container not found", "");

        return View("Container", container);
    }

    public IActionResult RemoveContainer(Guid id)
    {
        var list = GetServiceAddressList();
        if(list == null || list.Count == 0)
            return RedirectToAction(nameof(ServiceAddress))
                .WithDanger("Service addresses not found", "");

        ContainerViewModel container = null;
        foreach (var sa in list)
        {
            container = sa.Containers.FirstOrDefault(c => c.Id == id);
            if (container != null)
            {
                sa.Containers.Remove(container);
                break; 
            }
        }

        if(container == null)
            return RedirectToAction(nameof(ServiceAddress))
                .WithDanger("Container not found", "");

        SetServiceAddressList(list);
        return RedirectToAction(nameof(ServiceAddress))
            .WithSuccess("Container removed", "");
    }

    public async Task<IActionResult> SaveContainer(ContainerViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Container", model)
                .WithDanger("There are errors on the form", "");

        var rates = await rateService.GetByCodeDaysSizeEffDate(
            model.ContainerCodeId,
            model.ContainerSubtypeId,
            model.DaysCount(),
            model.BillingSize,
            model.EffectiveDate);
        if (!rates.Any())
            return View("Container", model)
                .WithDanger("Container rate not found", "");

        var serviceAddressList = GetServiceAddressList();

        var serviceAddress = serviceAddressList.FirstOrDefault(a => a.Id == model.ServiceAddressId);
        if (serviceAddress == null)
        {
            return RedirectToAction(nameof(ServiceAddress))
                .WithDanger("Service address not found", "");
        }
        if (model.EffectiveDate < serviceAddress.EffectiveDate)
        {
            return View("Container", model).WithDanger("Container effective date before service address effective date", "");
        }

        var container = serviceAddress.Containers.FirstOrDefault(c => c.Id == model.Id);
        if (container == null)
        {
            container = new ContainerViewModel
            {
                Id = model.Id,
                ServiceAddressId = serviceAddress.Id
            };
            serviceAddress.Containers.Add(container);
        }

        container.RouteNumber = model.RouteNumber;
        container.Delivered = model.Delivered;
        container.ActualSize = model.ActualSize;
        container.BillingSize = model.BillingSize;
        container.ContainerCodeId = model.ContainerCodeId;
        container.ContainerSubtypeId = model.ContainerSubtypeId;
        container.EffectiveDate = model.EffectiveDate;
        container.FriService = model.FriService;
        container.MonService = model.MonService;
        container.SatService = model.SatService;
        container.AdditionalCharge = model.AdditionalCharge;
        container.ThuService = model.ThuService;
        container.TueService = model.TueService;
        container.WedService = model.WedService;

        SetServiceAddressList(serviceAddressList);

        return RedirectToAction(nameof(serviceAddress))
            .WithSuccess("Container saved", "");
    }

    [HttpPost]
    public async Task<IActionResult> ContainerTypeChanged(ContainerViewModel model)
    {
        var containerCode = await containerCodeService.GetById(model.ContainerCodeId);
        if (containerCode == null)
            return NotFound();

        var html = await this.RenderViewAsync("SubtypeSelect", model);

        Dictionary<string, object> dict = new()
        {
            { "subtypeSelect", html }
        };

        if(containerCode.Type == "R")
        {
            var serviceAddressList = GetServiceAddressList();
            var serviceAddress = serviceAddressList.FirstOrDefault(a => a.Id == model.ServiceAddressId);
            var serviceAddressLine = serviceAddress != null ?
                $"{serviceAddress.AddressLine1} {serviceAddress.AddressLine2}" :
                null;

            dict.Add("routeInfo", true);
            if (serviceAddressLine != null)
            {
                try
                {
                    var results = await routeService.SearchRefuseRoute(serviceAddressLine);

                    if (results.Candidates.Count > 0)
                    {
                        var a = results.Candidates[0].Attributes;
                        dict.Add("routeNum", a.Route);
                        dict.Add("isMon", a.IsMonday);
                        dict.Add("isTue", a.IsTuesday);
                        dict.Add("isWed", a.IsWednesday);
                        dict.Add("isThu", a.IsThursday);
                        dict.Add("isFri", a.IsFriday);
                        dict.Add("isSat", a.IsSaturday);
                        dict.Add("isSun", a.IsSunday);
                        dict.Add("isRed", a.IsRed);
                        dict.Add("isBlue", a.IsBlue);
                    }
                    if (!dict.ContainsKey("routeNum"))
                    {
                        dict.Add("routeNum", "");
                        dict.Add("isMon", false);
                        dict.Add("isTue", false);
                        dict.Add("isWed", false);
                        dict.Add("isThu", false);
                        dict.Add("isFri", false);
                        dict.Add("isSat", false);
                        dict.Add("isSun", false);
                        dict.Add("isRed", false);
                        dict.Add("isBlue", false);
                    }
                }
                catch (Exception)
                {
                    dict.Add("routeNum", "");
                    dict.Add("isMon", false);
                    dict.Add("isTue", false);
                    dict.Add("isWed", false);
                    dict.Add("isThu", false);
                    dict.Add("isFri", false);
                    dict.Add("isSat", false);
                    dict.Add("isSun", false);
                    dict.Add("isRed", false);
                    dict.Add("isBlue", false);
                }
            }
        }
        else
        {
            dict.Add("routeInfo", false);
        }

        return Json(dict);
    }

    [HttpPost]
    public IActionResult ContainerBillingSize(ContainerViewModel model)
    {
        return PartialView("BillingSizeSelect", model);
    }

    private static ContainerViewModel GetContainer(ServiceAddressList list, Guid containerId)
    {
        if (list == null || list.Count == 0)
            return null;

        foreach(var sa in list)
        {
            var container = sa.Containers.FirstOrDefault(c => c.Id == containerId);
            if (container != null)
                return container;
        }

        return null;
    }

    #endregion

    #region Note

    public IActionResult AddNote(Guid id)
    {
        var address = GetServiceAddressList().FirstOrDefault(a => a.Id == id);
        if (address == null)
            return NotFound();

        NoteViewModel model = new()
        {
            Id = Guid.NewGuid(),
            Note = "",
            ServiceAddressId = address.Id
        };
        return View("Note", model);
    }

    public IActionResult EditNote(NoteViewModel model)
    {
        var address = GetServiceAddressList().FirstOrDefault(a => a.Id == model.ServiceAddressId);
        if (address == null)
            return NotFound();

        var note = address.Notes.FirstOrDefault(n => n.Id == model.Id);
        if (note == null)
            return NotFound();

        return View("Note", model);
    }

    public IActionResult RemoveNote(Guid serviceAddressId, Guid id)
    {
        var addresses = GetServiceAddressList();
        var address = addresses.FirstOrDefault(a => a.Id == serviceAddressId);
        if (address == null)
            return NotFound();

        var note = address.Notes.FirstOrDefault(n => n.Id == id);
        if (note == null)
            return RedirectToAction(nameof(ServiceAddress));

        address.Notes.Remove(note);
        SetServiceAddressList(addresses);
        return RedirectToAction(nameof(ServiceAddress))
            .WithSuccess("Note removed", "");
    }

    public IActionResult SaveNote(NoteViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Note", model).WithDanger("Invalid Customer Note", "");

        var addresses = GetServiceAddressList();
        var address = addresses.FirstOrDefault(a => a.Id == model.ServiceAddressId);
        if (address == null)
            return RedirectToAction(nameof(ServiceAddress)).WithDanger("Address not found", "");

        var note = address.Notes.FirstOrDefault(n => n.Id == model.Id);
        if (note == null)
        {
            address.Notes.Add(new()
            {
                Id = model.Id,
                Note = model.Note,
                ServiceAddressId = model.ServiceAddressId
            });
        }
        else
        {
            note.Note = model.Note;
        }

        SetServiceAddressList(addresses);

        return RedirectToAction(nameof(EditAddress), new { id = model.ServiceAddressId })
            .WithSuccess("Note saved", "");
    }

    #endregion

    #region Summary

    public async Task<IActionResult> Summary()
    {
        SummaryViewModel model = new()
        {
            BillingAddress = GetBillingAddress(),
            Customer = GetCustomer(),
            Email = GetEmail(),
            Phone = GetPhone(),
            ServiceAddresses = GetServiceAddressList()
        };

        Dictionary<Guid, decimal> container2rate = new();
        var containers = model.ServiceAddresses
            .SelectMany(a => a.Containers)
            .ToArray();

        foreach(var container in containers)
        {
            var rate = (await rateService.GetByCodeDaysSizeEffDate(
                container.ContainerCodeId,
                container.ContainerSubtypeId,
                container.DaysCount(),
                container.BillingSize,
                container.EffectiveDate))
                .FirstOrDefault();

            var amount = (rate?.RateAmount ?? 0.00m) + container.AdditionalCharge;

            container2rate.Add(container.Id, amount);
        }
        model.Container2Rate = container2rate;

        return View(model);
    }

    #endregion

    #region Done

    private async Task<PersonEntity> InsertPersonEntity(CustomerViewModel model)
    {
        var code = await codeService.Get("Department", "SW");

        PersonEntity pe = new()
        {
            SystemCode = code.Id,
            Status = true,
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            // Account handled by Add
            FullName = model.FullName,
            NameTypeFlag = model.NameTypeFlag,
            WhenCreated = DateTime.Now,
        };

        await personService.Add(pe);
        return pe;
    }

    private async Task<Address> InsertBillingAddress(BillingAddressViewModel model, PersonEntity pe)
    {
        var code = await codeService.Get("Address", "B");

        Address billingAddress = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            PersonEntityID = pe.Id,
            IsDefault = true,
            Type = code.Id,
            Apt = model.AddressLine2,
            Direction = null,
            Number = null,
            Override = model.Override,
            State = model.State,
            StreetName = model.AddressLine1,
            Suffix = null,
            Zip = model.Zip,
            City = model.City
        };

        await addressService.Add(billingAddress);
        return billingAddress;
    }

    private async Task<Customer> InsertCustomer(CustomerViewModel model, PersonEntity pe)
    {
        var parsedCC = decimal.TryParse(model.ContractCharge, out decimal contractCharge);
        var nextCustomerNumber = await customerService.GetNextCustomerNumber(model.CustomerType);

        Customer customer = new()
        {
            Active = true,
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            Contact = model.Contact,
            ContractCharge = parsedCC ? contractCharge : null,
            CustomerId = nextCustomerNumber,
            CustomerType = model.CustomerType,
            EffectiveDate = model.EffectiveDate.Value,
            NameAttn = model.NameAttn,
            Notes = model.Notes,
            Pe = pe.Id,
            PurchaseOrder = model.PurchaseOrder
        };

        await customerService.Add(customer);
        return customer;
    }

    private async Task InsertPhone(PhoneNumberViewModel model, PersonEntity pe)
    {
        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            return;

        Phone phone = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            IsDefault = true,
            PersonEntityID = pe.Id,
            PhoneNumber = model.PhoneNumber,
            Type = model.Type,
            Status = true
        };

        await phoneService.Add(phone);
    }

    private async Task InsertEmail(EmailViewModel model, PersonEntity pe)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
            return;

        //SCMB-243-New-Container-Rates-For-2022
        Email email = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            Email1 = model.Email.ToLower(),
            Status = true,
            IsDefault = true,
            PersonEntityID = pe.Id,
            Type = model.Type
        };

        await emailService.Add(email);
    }

    private async Task InsertServiceAddresses(ServiceAddressList serviceAddresses, PersonEntity pe, Customer customer)
    {
        foreach (var sam in serviceAddresses)
        {
            var address = await InsertAddress(sam, pe);
            var serviceAddress = await InsertServiceAddress(sam, customer, address);
            await InsertContainers(sam.Containers, serviceAddress);
            await InsertNotes(sam.Notes, serviceAddress);
        }
    }

    private async Task<Address> InsertAddress(ServiceAddressViewModel sam, PersonEntity pe)
    {
        var code = await codeService.Get("Address", "S");

        Address address = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            PersonEntityID = pe.Id,
            Type = code.Id,
            Number = null,
            Direction = null,
            StreetName = sam.AddressLine1,
            Suffix = null,
            Apt = sam.AddressLine2,
            City = sam.City,
            State = sam.State,
            Zip = sam.Zip,
            Override = sam.Override,
        };

        await addressService.Add(address);
        return address;
    }

    private async Task<ServiceAddress> InsertServiceAddress(ServiceAddressViewModel model, Customer customer, Address address)
    {
        var sa = new ServiceAddress
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            CustomerId = customer.CustomerId,
            CustomerType = customer.CustomerType,
            Email = model.Email,
            PeaddressId = address.Id,
            Phone = model.Phone,
            EffectiveDate = model.EffectiveDate,
            LocationContact = model.LocationContact,
            LocationName = model.LocationName
            // LocationNumber is in Add(sa)
        };

        await serviceAddressService.Add(sa);
        return sa;
    }

    private async Task InsertContainers(IEnumerable<ContainerViewModel> containers, ServiceAddress serviceAddress)
    {
        foreach (var cvm in containers)
        {
            Container c = new()
            {
                AddDateTime = DateTime.Now,
                AddToi = User.GetNameOrEmail(),
                ServiceAddressId = serviceAddress.Id,
                RouteNumber = cvm.RouteNumber,
                Delivered = cvm.Delivered,
                ActualSize = cvm.ActualSize,
                BillingSize = cvm.BillingSize,
                ContainerCodeId = cvm.ContainerCodeId,
                ContainerSubtypeId = cvm.ContainerSubtypeId,
                EffectiveDate = cvm.EffectiveDate,
                FriService = cvm.FriService,
                MonService = cvm.MonService,
                SatService = cvm.SatService,
                AdditionalCharge = cvm.AdditionalCharge,
                ThuService = cvm.ThuService,
                TueService = cvm.TueService,
                WedService = cvm.WedService
            };

            await containerService.Add(c);
        }
    }

    private async Task InsertNotes(IEnumerable<NoteViewModel> notes, ServiceAddress sa)
    {
        foreach (var nvm in notes)
        {
            ServiceAddressNote n = new()
            {
                AddDateTime = DateTime.Now,
                AddToi = User.GetNameOrEmail(),
                Note = nvm.Note,
                ServiceAddressId = sa.Id
            };

            await noteService.Add(n);
        }
    }

    private async Task SendEmail(
        EmailViewModel emailModel,
        CustomerViewModel customerModel,
        Address billingAddress,
        PhoneNumberViewModel phoneModel,
        ServiceAddressList serviceAddresses)
    {
        if (string.IsNullOrWhiteSpace(emailModel.Email))
            return;

        SendEmailDto email = new()
        {
            Subject = "Email Verification",
            TextContent = CreateEmailConfirmationBody(customerModel, billingAddress, phoneModel, emailModel, serviceAddresses)
        };
        email.AddTo(emailModel.Email);
        // use default from

        await sendGridService.SendSingleEmail(email);
    }

    private static string CreateEmailConfirmationBody(
        CustomerViewModel customerModel,
        Address billingAddress,
        PhoneNumberViewModel phoneModel,
        EmailViewModel emailModel,
        ServiceAddressList serviceAddresses)
    {
        var builder = new StringBuilder(4096)
            .Append("Hello ").Append(customerModel.FullName).Append(", \r\n\n")
            .Append("   Welcome to Shawnee County Solid waste.  In order to provide you the best service possible, \r\n")
            .Append("please take a moment to review the setup of your new account.  If you find any descrepancies \r\n")
            .Append("please call us at (785) 233-4774, so that we can make any required changes.  Thank you for your time. ")
            .Append("\r\n\n Customer Type:   ").Append(customerModel.CustomerType)
            .Append("\r\n Effective Date:  ").Append(string.Format("{0:d}", customerModel.EffectiveDate))
            .Append("\r\n Customer Name:   ").Append(customerModel.FullName)
            .Append("\r\n Attention Name:  ").Append(customerModel.NameAttn)
            .Append("\r\n Contact Name:    ").Append(customerModel.Contact)
            .Append("\r\n PO Number:       ").Append(customerModel.PurchaseOrder)
            .Append("\r\n\n Billing Address: ").Append(string.Format("{0} {1} {2} {3} {4}", billingAddress.Number, billingAddress.Direction, billingAddress.StreetName, billingAddress.Suffix, billingAddress.Apt).Trim())
            .Append("\r\n                    ").Append(string.Format("{0} {1} {2}", billingAddress.City, billingAddress.State, billingAddress.Zip).Trim())
            .Append("\r\n\n Phone Number:    ").Append(phoneModel.PhoneNumber)
            .Append("\r\n\n Email Address    ") .Append(emailModel.Email);

        foreach (var sam in serviceAddresses)
        {
            builder
                .Append("\r\n\n Service Address: ").Append(string.Format("{0} {1}", sam.AddressLine1, sam.AddressLine2).Trim())
                .Append("\r\n                    ").Append(string.Format("{0} {1} {2}", sam.City, sam.State, sam.Zip).Trim());

            foreach (var cvm in sam.Containers)
            {
                builder
                    .Append("\r\n\n Route Number:    ").Append(cvm.RouteNumber)
                    .Append("\r\n Actual Size:     ").Append(cvm.ActualSize)
                    .Append("\r\n Billing Size:    ").Append(cvm.BillingSize)
                    .Append("\r\n Effective Date:  ").Append(string.Format("{0:d}", cvm.EffectiveDate))
                    .Append("\r\n Additional Fee:  ").Append(cvm.AdditionalCharge)
                    .Append("\r\n Monday Service:    ").Append(cvm.MonService)
                    .Append("\r\n Tuesday Service:   ").Append(cvm.TueService)
                    .Append("\r\n Wednesday Service: ").Append(cvm.WedService)
                    .Append("\r\n Thursday Service:  ").Append(cvm.ThuService)
                    .Append("\r\n Friday Service:    ").Append(cvm.FriService)
                    .Append("\r\n Saturday Service:  ").Append(cvm.SatService);
            }
        }
        return builder.ToString();
    }

    public async Task<IActionResult> Done()
    {
        var customerModel = GetCustomer();
        var billingAddressModel = GetBillingAddress();
        var phoneModel = GetPhone();
        var emailModel = GetEmail();
        var serviceAddresses = GetServiceAddressList();

        if (customerModel == null)
            return RedirectToAction(nameof(Summary)).WithDanger("Customer information not found", "");

        if (billingAddressModel == null)
            return RedirectToAction(nameof(Summary)).WithDanger("Billing address not found", "");

        if (phoneModel == null)
            return RedirectToAction(nameof(Summary)).WithDanger("Phone number not found", "");

        if (emailModel == null)
            return RedirectToAction(nameof(Summary)).WithDanger("Email not found", "");

        if (serviceAddresses == null || !serviceAddresses.IsValid)
            return RedirectToAction(nameof(Summary)).WithDanger("Service address not found", "");


        if (!TryValidateModel(customerModel))
            return RedirectToAction(nameof(Summary)).WithDanger("Customer information is invalid", "");

        if (!TryValidateModel(billingAddressModel))
            return RedirectToAction(nameof(Summary)).WithDanger("Billing address is invalid", "");
        
        if (!TryValidateModel(phoneModel))
            return RedirectToAction(nameof(Summary)).WithDanger("Phone is invalid", "");
        
        if (!TryValidateModel(emailModel))
            return RedirectToAction(nameof(Summary)).WithDanger("Email is invalid", "");
        
        if (!TryValidateModel(serviceAddresses))
            return RedirectToAction(nameof(Summary)).WithDanger("Service address is invalid", "");

        try
        {
            var pe = await InsertPersonEntity(customerModel);
            var customer = await InsertCustomer(customerModel, pe);
            var billingAddress = await InsertBillingAddress(billingAddressModel, pe);
            await InsertPhone(phoneModel, pe);
            await InsertEmail(emailModel, pe);
            await InsertServiceAddresses(serviceAddresses, pe, customer);
            await SendEmail(emailModel, customerModel, billingAddress, phoneModel, serviceAddresses);

            RemoveCustomer();
            RemoveBillingAddress();
            RemoveValidBillingAddresses();
            RemovePhone();
            RemoveEmail();
            RemoveServiceAddressList();
            RemoveValidServiceAddresses();

            return RedirectToAction("Index", "Customer", new { customerId = customer.CustomerId });
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Summary)).WithDanger(ex.Message, "");
        }
    }

    #endregion

    #region Util

    private T Get<T>(string key)
        where T : class
    {
        var str = HttpContext.Session.GetString(key);
        return string.IsNullOrWhiteSpace(str) ? null : JsonSerializer.Deserialize<T>(str);
    }

    private void Set(string key, object obj)
    {
        HttpContext.Session.SetString(key, JsonSerializer.Serialize(obj));
    }

    private void Remove(string key)
    {
        HttpContext.Session.Remove(key);
    }

    #endregion
}
