﻿using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.ContainerRates;

namespace SW.InternalWeb.Controllers;

public class ContainerRateController : Controller
{
    private readonly IContainerRateService containerRateService;
    private readonly IContainerCodeService containerCodeService;
    private readonly IContainerSubtypeService containerSubtypeService;

    public ContainerRateController(
        IContainerRateService containerRateService,
        IContainerCodeService containerCodeService,
        IContainerSubtypeService containerSubtypeService)
    {
        this.containerRateService = containerRateService;
        this.containerCodeService = containerCodeService;
        this.containerSubtypeService = containerSubtypeService;
    }

    public async Task<IActionResult> Index()
    {
        var rates = await containerRateService.GetListing();
        return View(rates);
    }

    #region Add

    [HttpGet]
    public IActionResult Add()
    {
        return View(new ContainerRateAddViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Add(ContainerRateAddViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm).WithDanger("There are errors on the form", "");

        ContainerRate containerRate = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            BillingSize = vm.BillingSize.Value,
            ContainerSubtypeId = vm.ContainerSubtype,
            ContainerType = vm.ContainerType,
            EffectiveDate = vm.EffectiveDate.Value,
            NumDaysService = vm.NumDaysService,
            PullCharge = decimal.Parse(vm.PullCharge.Replace("$", "").Replace(",", "")),
            RateAmount = decimal.Parse(vm.RateAmount.Replace("$", "").Replace(",", "")),
            RateDescription = vm.RateDescription.ToUpper()
        };

        await containerRateService.Add(containerRate);

        return RedirectToAction(nameof(Edit), new { id = containerRate.ContainerRateId }).WithSuccess("Add Successful", "");
    }

    #endregion

    #region Edit

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var rate = await containerRateService.GetById(id);
        if (rate == null || rate.DeleteFlag)
            return RedirectToAction(nameof(Index)).WithDanger("Invalid container rate", "");

        var model = new ContainerRateEditViewModel
        {
            ContainerRateID = rate.ContainerRateId,
            BillingSize = rate.BillingSize,
            ContainerSubtype = rate.ContainerSubtypeId,
            ContainerType = rate.ContainerType,
            EffectiveDate = rate.EffectiveDate,
            NumDaysService = rate.NumDaysService,
            PullCharge = rate.PullCharge.ToString("0.00"),
            RateAmount = rate.RateAmount.ToString("0.00"),
            RateDescription = rate.RateDescription
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ContainerRateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model).WithDanger("Invalid container rate", "");

        var rate = await containerRateService.GetById(model.ContainerRateID);
        if (rate == null || rate.DeleteFlag)
            return RedirectToAction(nameof(Index)).WithDanger("Invalid container rate", "");

        rate.ChgDateTime = DateTime.Now;
        rate.ChgToi = User.GetNameOrEmail();
        rate.BillingSize = model.BillingSize ?? 0.0m;
        rate.ContainerSubtypeId = model.ContainerSubtype;
        rate.ContainerType = model.ContainerType;
        rate.EffectiveDate = model.EffectiveDate.Value;
        rate.NumDaysService = model.NumDaysService;
        rate.PullCharge = decimal.Parse(model.PullCharge.Replace("$", "").Replace(",", ""));
        rate.RateAmount = decimal.Parse(model.RateAmount.Replace("$", "").Replace(",", ""));
        rate.RateDescription = model.RateDescription;

        await containerRateService.Update(rate);

        return RedirectToAction(nameof(Edit), new { id = model.ContainerRateID }).WithSuccess("Update Successful", "");
    }

    #endregion

    #region Delete

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var containerRate = await containerRateService.GetById(id);
        if(containerRate == null || containerRate.DeleteFlag)
            return RedirectToAction(nameof(Index)).WithDanger("Invalid container rate", "");

        containerRate.DelDateTime = DateTime.Now;
        containerRate.DeleteFlag = true;
        containerRate.DelToi = User.GetNameOrEmail();
        await containerRateService.Delete(containerRate);

        return RedirectToAction("Index").WithSuccess("Delete Successful", "");
    }

    #endregion

    [HttpPost]
    public async Task<IActionResult> ContainerTypeChanged(int containerCodeId)
    {
        var containerType = await containerCodeService.GetById(containerCodeId);
        if (containerType == null)
            return NotFound();

        var subtypes = await containerSubtypeService.GetByContainerType(containerCodeId);
        var optionHtml = new System.Text.StringBuilder();
        foreach (var s in subtypes)
        {
            optionHtml.AppendLine($"<option value='{s.ContainerSubtypeId}'>{s.BillingFrequency} - {s.Description}</option>");
        }
        return Json(new { OptionHtml = optionHtml.ToString() });
    }
}
