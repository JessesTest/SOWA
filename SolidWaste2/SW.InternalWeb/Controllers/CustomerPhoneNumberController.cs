﻿using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.InternalWeb.Models.CustomerPhone;

namespace SW.InternalWeb.Controllers;

public class CustomerPhoneNumberController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personEntityService;
    private readonly IPhoneService phoneService;

    public CustomerPhoneNumberController(
        ICustomerService customerService,
        IPersonEntityService personEntityService,
        IPhoneService phoneService)
    {
        this.customerService = customerService;
        this.personEntityService = personEntityService;
        this.phoneService = phoneService;
    }

    public async Task<IActionResult> Index(int customerID, int? id)
    {
        var customer = await customerService.GetById(customerID);
        if (customer == null)
            return RedirectToAction("Index", "Customer")
                .WithDanger("Customer not found", "");

        var person = await personEntityService.GetById(customer.Pe);
        if (person == null)
            return RedirectToAction("Index", "Customer")
                .WithDanger("Customer record is invalid", "");

        Phone phone = null;
        if (id != null)
            phone = person.Phones.FirstOrDefault(p => p.Id == id);

        phone ??= person.Phones.FirstOrDefault(p => p.IsDefault);
        phone ??= person.Phones.FirstOrDefault();

        var phoneIndex = phone == null ? 0 : person.Phones.ToList().IndexOf(phone);

        CustomerPhoneNumberViewModel vm = new()
        {
            CurrentIndex = phoneIndex + 1,
            CustomerID = customer.CustomerId,
            CustomerType = customer.CustomerType,
            FullName = person.FullName,
            Id = phone?.Id,
            MaxIndex = person.Phones.Count,
            PhoneNumber = phone?.PhoneNumber,
            Status = phone?.Status ?? true,
            Type = phone?.Type ?? 3
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Update(CustomerPhoneNumberViewModel vm)
    {
        var customer = await customerService.GetById(vm.CustomerID);
        var person = await personEntityService.GetById(customer.Pe);

        if (!ModelState.IsValid)
            return View("Index", vm)
                .WithDanger("There were field validation errors", "");

        Phone phone = person.Phones.FirstOrDefault(p => p.Id == vm.Id);
        if (phone == null)
            return RedirectToAction("Index", "Customer", new { vm.CustomerID })
                .WithDanger("Phone number not found", "");

        phone.ChgDateTime = DateTime.Now;
        phone.ChgToi = User.GetNameOrEmail();
        phone.PhoneNumber = vm.PhoneNumber;
        phone.Status = vm.Status;
        phone.Type = vm.Type;

        phone.Code = null;
        await phoneService.Update(phone);

        return RedirectToAction(nameof(Index), new { customer.CustomerId, phone.Id })
            .WithSuccess("Phone number updated", "");
    }

    [HttpPost]
    public IActionResult Clear(CustomerPhoneNumberViewModel vm)
    {
        ModelState.Clear();
        vm.Id = null;
        vm.PhoneNumber = string.Empty;

        return View("Index", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CustomerPhoneNumberViewModel vm)
    {
        var customer = await customerService.GetById(vm.CustomerID);

        if (!ModelState.IsValid)
            return View("Index", vm)
                .WithDanger("There are errors on the form", "");

        Phone phone = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            PersonEntityID = customer.Pe,
            PhoneNumber = vm.PhoneNumber,
            Status = vm.Status,
            Type = vm.Type
        };

        await phoneService.Add(phone);

        return RedirectToAction(nameof(Index), new { vm.CustomerID, vm.Id })
            .WithSuccess("Phone number added", "");
    }

    [HttpPost]
    public async Task<IActionResult> Next(CustomerPhoneNumberViewModel vm)
    {
        var customer = await customerService.GetById(vm.CustomerID);
        var person = await personEntityService.GetById(customer.Pe);

        var phones = person.Phones.ToList();
        var phone = phones.FirstOrDefault(p => p.Id == vm.Id);
        var phoneIndex = phones.IndexOf(phone);
        var nextIndex = phoneIndex + 1;
        if (nextIndex >= phones.Count)
            nextIndex = 0;

        var nextPhone = phones[nextIndex];
        return RedirectToAction(nameof(Index), new { vm.CustomerID, nextPhone.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Previous(CustomerPhoneNumberViewModel vm)
    {
        var customer = await customerService.GetById(vm.CustomerID);
        var person = await personEntityService.GetById(customer.Pe);

        var phones = person.Phones.ToList();
        var phone = phones.FirstOrDefault(p => p.Id == vm.Id);
        var phoneIndex = phones.IndexOf(phone);
        var nextIndex = phoneIndex - 1;
        if (nextIndex < 0)
            nextIndex = phones.Count - 1;

        var prevPhone = phones[nextIndex];
        return RedirectToAction(nameof(Index), new { vm.CustomerID, prevPhone.Id });
    }
}
