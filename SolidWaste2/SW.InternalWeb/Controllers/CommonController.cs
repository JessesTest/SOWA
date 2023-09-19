using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;

namespace SW.InternalWeb.Controllers;

public class CommonController : Controller
{
    private readonly IBillingSummaryService billingSummaryService;
    private readonly IContainerRateService containerRateService;
    private readonly IContainerSubtypeService containerSubtypeService;
    private readonly IContainerCodeService containerCodeService;
    private readonly IRefuseRouteService refuseRouteService;

    public CommonController(
        IBillingSummaryService billingSummaryService,
        IContainerRateService containerRateService,
        IContainerSubtypeService containerSubtypeService,
        IContainerCodeService containerCodeService,
        IRefuseRouteService refuseRouteService)
    {
        this.billingSummaryService = billingSummaryService;
        this.containerRateService = containerRateService;
        this.containerSubtypeService = containerSubtypeService;
        this.containerCodeService = containerCodeService;
        this.refuseRouteService = refuseRouteService;
    }

    public async Task<IActionResult> BillingSummary(int customerId)
    {
        var model = await billingSummaryService.GetBillingSummary(customerId);
        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> ContainerBillingSizeChanged(
        int containerSubtypeId,
        int daysOfService,
        decimal size,
        DateTime? effectiveDate)
    {
        var effective_date = effectiveDate?.Date ?? DateTime.Today;
        if (effective_date > DateTime.Today)
            effective_date = DateTime.Today;

        var rate = (await containerRateService.GetByCodeDaysSize(containerSubtypeId, daysOfService, size, effective_date)).FirstOrDefault();
        if (rate == null)
        {
            return Json(new { amount = "n/a", frequency = "" });
        }

        var subtype = await containerSubtypeService.GetById(containerSubtypeId);
        return Json(new
        {
            amount = string.Format("{0:c}", rate?.RateAmount ?? 0),
            frequency = subtype.BillingFrequency
        });

    }

    [HttpPost]
    public async Task<IActionResult> ContainerSubtypeChanged(int containerSubtypeId)
    {
        var list = await containerRateService.GetDaysOfServiceByContainerSubtype(containerSubtypeId);
        return Json(new { daysOfService = list });
    }

    [NonAction]
    public async Task<IActionResult> ContainerTypeChanged(
        int containerCodeId,
        Func<string> generateAddressLine,
        Action<IDictionary<string, object>> generateSubtypes)
    {
        var containerType = await containerCodeService.GetById(containerCodeId);
        if (containerType == null)
            return NotFound();

        var dict = new Dictionary<string, object>();
        generateSubtypes(dict);

        if (containerType.Type == "R")
        {
            dict.Add("routeInfo", true);
            await ContainerTypeChanged_RInfo(dict, generateAddressLine);
        }
        else
        {
            dict.Add("routeInfo", false);
        }

        return Json(dict);
    }

    private async Task ContainerTypeChanged_RInfo(
        IDictionary<string, object> dict,
        Func<string> generateAddressLine)
    {
        var serviceAddressLine = generateAddressLine();
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
}
