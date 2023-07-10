using Common.Extensions;
using Common.Services.AddressValidation;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Extensions;
using SW.InternalWeb.Models.CustomerServiceAddress;
using System.Text;

namespace SW.InternalWeb.Controllers;

public class CustomerServiceAddressController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personService;
    private readonly IServiceAddressService serviceAddressService;
    private readonly IAddressService addressService;
    private readonly ICodeService codeService;
    private readonly IAddressValidationService addressValidationService;
    private readonly IContainerSubtypeService containerSubtypeService;
    private readonly IRefuseRouteService refuseRouteService;
    private readonly IContainerService containerService;
    private readonly IServiceAddressNoteService noteService;

    public CustomerServiceAddressController(
        ICustomerService customerService,
        IServiceAddressService serviceAddressService,
        IAddressService addressService,
        IPersonEntityService personEntityService,
        ICodeService codeService,
        IAddressValidationService addressValidationService,
        IContainerSubtypeService containerSubtypeService,
        IRefuseRouteService refuseRouteService,
        IContainerService containerService,
        IServiceAddressNoteService noteService)
    {
        this.customerService = customerService;
        this.serviceAddressService = serviceAddressService;
        this.addressService = addressService;
        this.personService = personEntityService;
        this.codeService = codeService;
        this.addressValidationService = addressValidationService;
        this.containerSubtypeService = containerSubtypeService;
        this.refuseRouteService = refuseRouteService;
        this.containerService = containerService;
        this.noteService = noteService;
    }

    #region ServiceAddress

    public async Task<IActionResult> Index(int customerId)
    {
        var customer = await customerService.GetById(customerId);
        if (customer == null)
            return RedirectToAction("Index", "Customer")
                .WithDanger("Customer not found", "");

        var addresses = await serviceAddressService
            .GetByCustomer(customer.CustomerId);
        
        var address = addresses
            .OrderBy(a => a.CancelDate)
            .ThenByDescending(a => a.LocationNumber)
            .FirstOrDefault();

        return RedirectToAction(nameof(EditAddress), new { id = address?.Id });
    }

    public async Task<IActionResult> EditAddress(int id)
    {
        var serviceAddress = await serviceAddressService.GetById(id);
        if (serviceAddress == null)
            return RedirectToAction("Index", "Customer").WithDanger("Service address not found", "");

        var serviceAddresses = (await serviceAddressService.GetByCustomer(serviceAddress.CustomerId))
            .OrderBy(a => a.CancelDate)
            .ThenByDescending(a => a.LocationNumber)
            .ToList();

        var customer = await customerService.GetById(serviceAddress.CustomerId);
        var person = await personService.GetById(customer.Pe);
        var address = person.Addresses.First(a => a.Id == serviceAddress.PeaddressId);
        var note = serviceAddress.ServiceAddressNotes.FirstOrDefault();
        var container = serviceAddress.Containers.FirstOrDefault();

        ContainerViewModel containerViewModel = container == null ?
            new()
            {
                EffectiveDate = (new[] { serviceAddress.EffectiveDate, customer.EffectiveDate, DateTime.Today.AddDays(1) }).Max()
            } :
            new()
            {
                ActualSize = container.ActualSize,
                AddDateTime = container.AddDateTime,
                AdditionalCharge = container.AdditionalCharge.ToString("0.00"),
                AddToi = container.AddToi,
                BillingSize = container.BillingSize,
                CancelDate = container.CancelDate,
                ChgDateTime = container.ChgDateTime,
                ChgToi = container.ChgToi,
                ContainerCodeId = container.ContainerCodeId,
                ContainerSubtypeID = container.ContainerSubtypeId,
                Delivered = container.Delivered,
                EffectiveDate = container.EffectiveDate,
                FriService = container.FriService,
                Id = container.Id,
                MonService = container.MonService,
                RouteNumber = container.RouteNumber,
                SatService = container.SatService,
                ThuService = container.ThuService,
                TueService = container.TueService,
                WedService = container.WedService
            };

        ServiceAddressMasterViewModel model = new()
        {
            AddressCount = serviceAddresses.Count,
            AddressIndex = serviceAddresses.IndexOf(serviceAddress) + 1,
            ServiceAddress = new()
            {
                AddressLine1 = address?.FormatAddressLine1(),
                AddressLine2 = address?.FormatAddressLine2(),
                City = address?.City,
                State = address?.State,
                Zip = address?.Zip,
                AddressOverride = address?.Override ?? false,
                CancelDate = serviceAddress.CancelDate,
                CustomerId = serviceAddress.CustomerId,
                CustomerType = serviceAddress.CustomerType,
                EffectiveDate = serviceAddress.EffectiveDate,
                Email = serviceAddress.Email,
                Id = serviceAddress.Id,
                LocationContact = serviceAddress.LocationContact,
                LocationName = serviceAddress.LocationName,
                Phone = serviceAddress.Phone
            },

            NoteCount = serviceAddress?.ServiceAddressNotes.Count ?? 0,
            NoteIndex = serviceAddress?.ServiceAddressNotes?.ToList().IndexOf(note) + 1 ?? 0,
            Note = new()
            {
                AddDateTime = note?.AddDateTime.ToString() ?? "",
                AddToi = note?.AddToi,
                Id = note?.Id ?? 0,
                Note = note?.Note
            },

            Container = containerViewModel,
            ContainerCount = serviceAddress.Containers.Count,
            ContainerIndex = 1,

            FullName = person.FullName,
            CustomerCancelDate = customer.CancelDate
        };

        ModelState.Clear();

        if (Request.IsAjaxRequest())
            return PartialView("Index", model);

        return View("Index", model)
            .WithDangerWhen(!serviceAddress.Containers.Any(), "No containers found", "")
            .WithDangerWhen(customer == null, "Could not find customer record!", "")
            .WithWarningWhen(person.Pab == true, "Account has undeliverable address.", "")
            .WithInfoWhen(customer?.PaymentPlan == true, "Customer has a payment plan.", "");
    }

    [HttpPost]
    public async Task<IActionResult> ClearAddress(ServiceAddressMasterViewModel model)
    {
        return await ClearAddress(model.ServiceAddress.CustomerId);
    }

    public async Task<IActionResult> ClearAddress(int customerId)
    {
        var customer = await customerService.GetById(customerId);
        if (customer == null)
            return NotFound();

        var serviceAddresses = await serviceAddressService.GetByCustomer(customerId);
        var person = await personService.GetById(customer.Pe);

        var possibleEffectiveDates = new[]
        {
            customer.EffectiveDate,
            DateTime.Today.AddDays(1)
        };

        ServiceAddressMasterViewModel model = new()
        {
            AddressCount = serviceAddresses.Count,
            AddressIndex = 0,
            FullName = person.FullName,
            CustomerCancelDate = customer.CancelDate,
            ServiceAddress = new()
            {
                CustomerId = customer.CustomerId,
                CustomerType = customer.CustomerType,
                EffectiveDate = possibleEffectiveDates.Max(),
                State = "KS"
            }
        };

        ModelState.Clear();
        return View("Index", model)
            .WithWarningWhen(person.Pab == true, "Account has undeliverable address.", "")
            .WithInfoWhen(customer?.PaymentPlan == true, "Customer has a payment plan.", "");
    }

    [HttpPost]
    public async Task<IActionResult> ReactivateCustomer(ServiceAddressMasterViewModel model)
    {
        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);
        customer.CancelDate = null;
        customer.ChgDateTime = DateTime.Now;
        customer.ChgToi = User.GetNameOrEmail();
        await customerService.Update(customer);

        return RedirectToAction("Index", new { customerId = model.ServiceAddress.CustomerId })
            .WithSuccess("Customer reactivated", "");
    }

    [HttpPost]
    public async Task<IActionResult> ReactivateAddress(ServiceAddressMasterViewModel model)
    {
        var serviceAddress = await serviceAddressService.GetById(model.ServiceAddress.Id);
        if (serviceAddress == null)
            return RedirectToAction(nameof(Index), new { customerId = model.ServiceAddress.CustomerId })
                .WithWarning("Service address not found", "");

        serviceAddress.ChgDateTime = DateTime.Now;
        serviceAddress.ChgToi = User.GetNameOrEmail();
        serviceAddress.CancelDate = null;
        await serviceAddressService.Update(serviceAddress);

        return RedirectToAction("Index", new { customerId = model.ServiceAddress.CustomerId })
            .WithSuccess("Address reactivated", "");
    }

    [HttpPost]
    public async Task<IActionResult> SaveAddress(ServiceAddressMasterViewModel model)
    {
        return await SaveAddressInternal(model);
    }

    private async Task<IActionResult> SaveAddressInternal(ServiceAddressMasterViewModel model)
    {
        if (!TryValidateModel(model.ServiceAddress))
            return View("Index", model).WithDanger("Invalid Service Address", "");

        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);

        ServiceAddress serviceAddress;
        if (model.ServiceAddress.Id > 0)
        {
            serviceAddress = await serviceAddressService.GetById(model.ServiceAddress.Id);
            serviceAddress.ChgDateTime = DateTime.Now;
            serviceAddress.ChgToi = User.GetNameOrEmail();
            serviceAddress.EffectiveDate = model.ServiceAddress.EffectiveDate;
            serviceAddress.CancelDate = model.ServiceAddress.CancelDate;
            serviceAddress.Email = model.ServiceAddress.Email;
            serviceAddress.LocationContact = model.ServiceAddress.LocationContact?.ToUpper();
            serviceAddress.LocationName = model.ServiceAddress.LocationName?.ToUpper();
            serviceAddress.Phone = model.ServiceAddress.Phone?.ToUpper();
        }
        else
        {
            serviceAddress = new()
            {
                AddDateTime = DateTime.Now,
                AddToi = User.GetNameOrEmail(),
                CustomerId = customer.CustomerId,
                CustomerType = customer.CustomerType,
                EffectiveDate = model.ServiceAddress.EffectiveDate,
                Email = model.ServiceAddress.Email,
                LocationContact = model.ServiceAddress.LocationContact?.ToUpper(),
                LocationName = model.ServiceAddress.LocationName?.ToUpper(),
                //LocationNumber assigned in add
                Phone = model.ServiceAddress.Phone?.ToUpper(),
                //ServiceType not used
                CancelDate = model.ServiceAddress.CancelDate
            };
        }

        Address address;
        if (serviceAddress.PeaddressId == 0)
        {
            var peAddressType = await codeService.Get("Address", "S");

            address = new()
            {
                AddDateTime = DateTime.Now,
                AddToi = User.GetNameOrEmail(),
                Override = model.ServiceAddress.AddressOverride,
                PersonEntityID = customer.Pe, //person.Id,
                StreetName = model.ServiceAddress.AddressLine1?.ToUpper(),
                Apt = model.ServiceAddress.AddressLine2?.ToUpper(),
                City = model.ServiceAddress.City?.ToUpper(),
                State = model.ServiceAddress.State?.ToUpper(),
                Zip = model.ServiceAddress.Zip?.ToUpper(),
                Type = peAddressType.Id
                //Direction = null,
                //Number = null,
                //Suffix = null
            };
        }
        else
        {
            address = await addressService.GetById(serviceAddress.PeaddressId);
            address.ChgDateTime = DateTime.Now;
            address.ChgToi = User.GetNameOrEmail();
            address.StreetName = model.ServiceAddress.AddressLine1?.ToUpper();
            address.Apt = model.ServiceAddress.AddressLine2?.ToUpper();
            address.City = model.ServiceAddress.City?.ToUpper();
            address.State = model.ServiceAddress.State?.ToUpper();
            address.Zip = model.ServiceAddress.Zip?.ToUpper();
            //address.Type
            address.Direction = null;
            address.Number = null;
            address.Override = model.ServiceAddress.AddressOverride;
            address.Suffix = null;
        }

        if (serviceAddress.CancelDate != null && serviceAddress.CancelDate.Value <= DateTime.Today)
            return View("Index", model).WithDanger("Cancel date is before today", "");

        var additionalError = await serviceAddressService.TryValidateServiceAddress(serviceAddress);
        if (additionalError != null)
            return View("Index", model).WithDanger(additionalError, "");

        // validate address
        if (model.ServiceAddress.AddressOverride || model.ServiceAddress.State.ToUpper() != "KS")
        {
            if (string.IsNullOrWhiteSpace(address.Zip))
                return View("Index", model).WithDanger("Address Override", "Zip code required");
            if (string.IsNullOrWhiteSpace(address.State))
                return View("Index", model).WithDanger("Address Override", "State required");
            if (string.IsNullOrWhiteSpace(address.City))
                return View("Index", model).WithDanger("Address Override", "City required");
        }

        if (model.ServiceAddress.CancelDate.HasValue && model.ServiceAddress.CancelDate < DateTime.Today)
            return View("Index", model).WithDanger($"Cancel Date Before {DateTime.Today:d}", "");

        var validAddresses = await addressValidationService.GetCandidates(
            model.ServiceAddress.AddressLine1,
            model.ServiceAddress.City,
            model.ServiceAddress.Zip,
            8);
        if (validAddresses.Count == 0)
            return View("Index", model).WithDanger("Address not found", "");
        else if (validAddresses.Count == 1 || model.ServiceAddressListIndex != null)
        {
            var valid = validAddresses[model.ServiceAddressListIndex ?? 0];
            address.StreetName = valid.Address?.ToUpper();
            address.Apt = model.ServiceAddress.AddressLine2?.ToUpper();
            address.City = valid.City?.ToUpper();
            address.State = valid.State?.ToUpper();
            address.Zip = valid.Zip?.ToUpper();

            model.ServiceAddress.AddressLine1 = valid.Address;
            model.ServiceAddress.City = valid.City;
            model.ServiceAddress.State = valid.State;
            model.ServiceAddress.Zip = valid.Zip;
        }
        else
        {
            model.ServiceAddressList = validAddresses;
            return View("Index", model)
                .WithInfo("Select an adddress from the list", "");
        }

        if (address.Id > 0)
            await addressService.Update(address);
        else
            await addressService.Add(address);

        if (serviceAddress.PeaddressId <= 0)
            serviceAddress.PeaddressId = address.Id;

        if (serviceAddress.Id > 0)
            await serviceAddressService.Update(serviceAddress); // this will cancel containers if necessary 
        else
            await serviceAddressService.Add(serviceAddress);

        return RedirectToAction(nameof(EditAddress), new { id = serviceAddress.Id })
            .WithSuccess("Address updated", "");
    }

    [HttpPost]
    public async Task<IActionResult> SelectAddress(ServiceAddressMasterViewModel model)
    {
        return await SaveAddressInternal(model);
    }

    private async Task<IActionResult> PageAddress(int step, int customerId, int serviceAddressId)
    {
        var addresses = (await serviceAddressService.GetByCustomer(customerId))
            .OrderBy(a => a.CancelDate)
            .ThenByDescending(a => a.LocationNumber)
            .ToList();

        if (!addresses.Any())
            return RedirectToAction(nameof(ClearAddress), new { customerId });

        var address = addresses.FirstOrDefault(a => a.Id == serviceAddressId);

        if (address == null)
            return RedirectToAction(nameof(EditAddress), new { id = addresses[0].Id });

        var index = addresses.IndexOf(address);
        index += step;

        if (index < 0)
            index = addresses.Count - 1;
        else if (index >= addresses.Count)
            index = 0;

        return RedirectToAction(nameof(EditAddress), new { id = addresses[index].Id });
    }

    public async Task<IActionResult> NextAddress(ServiceAddressMasterViewModel model)
    {
        return await PageAddress(1, model.ServiceAddress.CustomerId, model.ServiceAddress.Id);
    }

    public async Task<IActionResult> PreviousAddress(ServiceAddressMasterViewModel model)
    {
        return await PageAddress(-1, model.ServiceAddress.CustomerId, model.ServiceAddress.Id);
    }

    #endregion

    #region Container

    public async Task<IActionResult> ContainerTypeChanged(int containerCodeId, int serviceAddressId, int? containerSubtypeId)
    {
        var containerCode = await codeService.GetById(containerCodeId);
        if (containerCode == null)
            return NotFound();

        var containerSubtypes = await containerSubtypeService.GetByContainerType(containerCodeId);
        var optionHtml = new StringBuilder();
        foreach(var s in containerSubtypes)
        {
            if (s.ContainerSubtypeId == containerSubtypeId)
                optionHtml.Append($"<option value=\"{s.ContainerSubtypeId}\" selected>{s.BillingFrequency} - {s.Description}</option>");
            else
                optionHtml.Append($"<option value=\"{s.ContainerSubtypeId}\">{s.BillingFrequency} - {s.Description}</option>");

        }
        Dictionary<string, object> dict = new();
        dict.Add("optionHtml", optionHtml.ToString());

        switch (containerCode.Type)
        {
            case "R":
                //case "Z":
                dict.Add("routeInfo", true);
                await ContainerTypeChanged_RInfo(dict, serviceAddressId);
                break;
            default:
                dict.Add("routeInfo", false);
                break;
        }

        return Json(dict);
    }

    private async Task ContainerTypeChanged_RInfo(Dictionary<string, object> dict, int serviceAddressId)
    {
        var serviceAddress = await serviceAddressService.GetById(serviceAddressId);
        var address = serviceAddress != null ?
            await addressService.GetById(serviceAddress.PeaddressId) :
            null;

        var serviceAddressLine = address?.FormatAddress();

        if (serviceAddressLine != null)
        {
            var results = await refuseRouteService.SearchRefuseRoute(serviceAddressLine);

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

    public IActionResult BillingSizeSelect(
        int containerCodeId,
        int containerSubtypeId,
        int dayCount,
        decimal currentValue,
        DateTime? effectiveDate)
    {
        ContainerViewModel cvm = new()
        {
            ContainerCodeId = containerCodeId,
            ContainerSubtypeID = containerSubtypeId,
            EffectiveDate = effectiveDate ?? DateTime.Today,
            BillingSize = currentValue
        };
        ServiceAddressMasterViewModel model = new()
        {
            Container = cvm
        };

        cvm.SatService = dayCount >= 6;
        cvm.FriService = dayCount >= 5;
        cvm.ThuService = dayCount >= 4;
        cvm.WedService = dayCount >= 3;
        cvm.TueService = dayCount >= 2;
        cvm.MonService = dayCount >= 1;

        return PartialView("BillingSizeSelect", model);
    }

    public async Task<IActionResult> NextContainer(ServiceAddressMasterViewModel model)
    {
        return await PageContainer(model, 1);
    }

    public async Task<IActionResult> PreviousContainer(ServiceAddressMasterViewModel model)
    {
        return await PageContainer(model, -1);
    }

    private async Task<IActionResult> PageContainer(ServiceAddressMasterViewModel model, int increment)
    {
        ModelState.Clear();
        var containers = (await containerService.GetByServiceAddress(model.ServiceAddress.Id))
            .OrderBy(m => m.CancelDate)
            .ToList();
        if(!containers.Any())
        {
            model.Container = new ContainerViewModel();
            model.ContainerCount = 0;
            model.ContainerIndex = 0;
        }
        else
        {
            var index = DisplayIndexOf(new Container() { Id = model.Container.Id }, containers) - 1 + increment;
            if (index < 0)
                index = containers.Count - 1;
            else if (index >= containers.Count)
                index = 0;

            var c = containers[index];
            ContainerViewModel cvm = new()
            {
                ActualSize = c.ActualSize,
                AddDateTime = c.AddDateTime,
                AdditionalCharge = c.AdditionalCharge.ToString("0.00"),
                AddToi = c.AddToi,
                BillingSize = c.BillingSize,
                CancelDate = c.CancelDate,
                ChgDateTime = c.ChgDateTime,
                ChgToi = c.ChgToi,
                ContainerCodeId = c.ContainerCodeId,
                ContainerSubtypeID = c.ContainerSubtypeId,
                Delivered = c.Delivered,
                EffectiveDate = c.EffectiveDate,
                FriService = c.FriService,
                Id = c.Id,
                MonService = c.MonService,
                RouteNumber = c.RouteNumber,
                SatService = c.SatService,
                ThuService = c.ThuService,
                TueService = c.TueService,
                WedService = c.WedService
            };
            model.Container = cvm;
            model.ContainerCount = containers.Count;
            model.ContainerIndex = index + 1;
        }

        if (Request.IsAjaxRequest())
            return PartialView("Container", model);

        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);

        return View("Index", model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "");
    }

    private static int DisplayIndexOf(Container container, IList<Container> containers)
    {
        for (var i = 0; i < containers.Count; i++)
        {
            if (container.Id == containers[i].Id)
                return i + 1;
        }
        return 0;
    }
    
    public async Task<IActionResult> ClearContainer(ServiceAddressMasterViewModel model)
    {
        ModelState.Clear();
        var containers = await containerService.GetByServiceAddress(model.ServiceAddress.Id);
        model.Container = new ContainerViewModel();
        model.Container.EffectiveDate = DateTime.Now;
        model.ContainerCount = containers.Count;
        model.ContainerIndex = 0;

        if (Request.IsAjaxRequest())
            return PartialView("Container", model);

        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);

        return View("Index", model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "");
    }

    public async Task<IActionResult> SaveContainer(ServiceAddressMasterViewModel model)
    {
        ModelState.Clear();
        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);
        string xAlertMessage = null;

        if (!TryValidateModel(model.Container))
        {
            xAlertMessage = "Invalid Container";
        }
        else
        {
            Container c;
            if (model.Container.Id > 0)
            {
                c = await containerService.GetById(model.Container.Id);
                c.ServiceAddress = null;
                c.ContainerCode = null;
                c.ContainerSubtype = null;
                c.ChgDateTime = DateTime.Now;
                c.ChgToi = User.GetNameOrEmail();
            }
            else
            {
                c = new Container();
                c.AddDateTime = DateTime.Now;
                c.AddToi = User.GetNameOrEmail();
                c.ServiceAddressId = model.ServiceAddress.Id;
            }
            c.Delivered = model.Container.Delivered;
            c.ContainerCodeId = model.Container.ContainerCodeId;
            c.ContainerSubtypeId = model.Container.ContainerSubtypeID;
            c.MonService = model.Container.MonService;
            c.TueService = model.Container.TueService;
            c.WedService = model.Container.WedService;
            c.ThuService = model.Container.ThuService;
            c.FriService = model.Container.FriService;
            c.SatService = model.Container.SatService;
            c.RouteNumber = model.Container.RouteNumber;
            c.BillingSize = model.Container.BillingSize;
            c.ActualSize = model.Container.ActualSize;
            c.AdditionalCharge = model.Container.AdditionalCharge == null ? 0 : decimal.Parse(model.Container.AdditionalCharge);

            if (model.Container.Id == 0 && model.Container.EffectiveDate < DateTime.Today.Date)
            {
                xAlertMessage = "Container Effective Date before " + DateTime.Today.Date.ToShortDateString();
            }
            else if (model.Container.Id > 0 && model.Container.EffectiveDate < DateTime.Today.Date && model.Container.EffectiveDate != c.EffectiveDate)
            {
                xAlertMessage = "Container Effective Date before " + DateTime.Today.Date.ToShortDateString();
            }
            else
            {
                c.EffectiveDate = model.Container.EffectiveDate;
            }

            if (model.Container.Id == 0 && model.Container.CancelDate < DateTime.Today.Date)
            {
                xAlertMessage = "Container Cancel Date before " + DateTime.Today.Date.ToShortDateString();
            }
            else if (model.Container.Id > 0 && model.Container.CancelDate < model.Container.EffectiveDate)
            {
                xAlertMessage = "Please, set Container Cancel Date, equal to, Container Effective Date ";
            }
            else if (model.Container.Id > 0 && model.Container.CancelDate < DateTime.Today.Date && model.Container.CancelDate != c.CancelDate)
            {
                xAlertMessage = "Container Cancel Date before " + DateTime.Today.Date.ToShortDateString();
            }
            else
            {
                c.CancelDate = model.Container.CancelDate;
            }

            if (ModelState.IsValid)
            {
                var msg = await containerService.TryValidateContainer(c);
                if (string.IsNullOrWhiteSpace(msg))
                {
                    if(c.Id > 0)
                    {
                        await containerService.Update(c);

                        model.Container.Id = c.Id;
                    }
                    else
                    {
                        await containerService.Add(c);

                        var containers = await containerService.GetByServiceAddress(model.ServiceAddress.Id);
                        model.ContainerCount = containers.Count;
                        model.ContainerIndex = DisplayIndexOf(c, containers.ToList());
                        model.Container.Id = c.Id;
                    }
                }
                else
                {
                    xAlertMessage = msg;
                }
            }
        }


        if (Request.IsAjaxRequest())
        {
            Response.AddXAlertMessage(xAlertMessage);
            return PartialView("Container", model);
        }

        return View("Index", model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "")
            .WithDangerWhen(!string.IsNullOrWhiteSpace(xAlertMessage), xAlertMessage, "");
    }

    #endregion

    #region Notes

    public async Task<IActionResult> NextNote(ServiceAddressMasterViewModel model)
    {
        return await PageNote(model, 1);
    }

    public async Task<IActionResult> PreviousNote(ServiceAddressMasterViewModel model)
    {
        return await PageNote(model, -1);
    }

    private async Task<IActionResult> PageNote(ServiceAddressMasterViewModel model, int indexIncrement)
    {
        ModelState.Clear();
        var notes = (await noteService.GetByServiceAddress(model.ServiceAddress.Id)).ToList();
        var index = IndexOf(model.Note.Id, notes);
        if(index >= 0)
        {
            index += indexIncrement;
            if (index < 0)
                index = notes.Count - 1;
            else if (index >= notes.Count)
                index = 0;
        }

        if (index >= 0)
        {
            var san = notes[index];
            model.NoteCount = notes.Count;
            model.NoteIndex = index + 1;
            model.Note = new ServiceAddressNoteViewModel
            {
                Id = san.Id,
                Note = san.Note,
                AddDateTime = san.AddDateTime.ToString(),
                AddToi = san.AddToi
            };
        }
        else
        {
            model.Note = new ServiceAddressNoteViewModel();
        }

        if (Request.IsAjaxRequest())
            return PartialView("Note", model);

        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);
        return View("Index", model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "");
    }

    private static int IndexOf(int id, IEnumerable<ServiceAddressNote> notes)
    {
        int i = 0;
        foreach(var note in notes)
        {
            if (note.Id == id)
                return i;

            i++;
        }
        return -1;
    }

    public async Task<IActionResult> ClearNote(ServiceAddressMasterViewModel model)
    {
        ModelState.Clear();
        model.Note = new ServiceAddressNoteViewModel();
        model.NoteIndex = 0;

        if (Request.IsAjaxRequest())
            return PartialView("Note", model);

        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);
        return View("Index", model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "");
    }

    public async Task<IActionResult> AddNote(ServiceAddressMasterViewModel model)
    {
        ModelState.Clear();
        var customer = await customerService.GetById(model.ServiceAddress.CustomerId);

        if (!TryValidateModel(model.Note))
        {
            if (Request.IsAjaxRequest())
                return PartialView("Note", model);

            return View("Index", model)
                .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "")
                .WithDanger("Invalid Container Code", "");
        }

        var note = new ServiceAddressNote
        {
            Note = model.Note.Note,
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            ServiceAddressId = model.ServiceAddress.Id
        };
        await noteService.Add(note);

        var notes = await noteService.GetByServiceAddress(model.ServiceAddress.Id);
        model.NoteIndex = 1;
        model.NoteCount = notes.Count;
        model.Note.Id = note.Id;
        model.Note.AddDateTime = note.AddDateTime.ToString();
        model.Note.AddToi = note.AddToi;

        if (Request.IsAjaxRequest())
            return PartialView("Note", model);
        
        return View("Index", model)
            .WithInfoWhen(customer.PaymentPlan, "Customer has a payment plan.", "")
            .WithSuccess("Note Added", "");
    }

    #endregion
}
