using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.InternalWeb.Models.WorkOrderLegacyInquiry;

namespace SW.InternalWeb.Controllers;

public class WorkOrderLegacyInquiryController : Controller
{
    private readonly IWorkOrderLegacyService _workOrderLegacyService;

    public WorkOrderLegacyInquiryController(IWorkOrderLegacyService workOrderLegacyService)
    {
        _workOrderLegacyService = workOrderLegacyService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var vm = new WorkOrderLegacyInquiryViewModel
        {
            Include = false
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Results(WorkOrderLegacyInquiryViewModel vm)
    {
        try
        {
            if (vm.EquipmentNumber == null && string.IsNullOrWhiteSpace(vm.Route) && vm.TransDate == null && string.IsNullOrWhiteSpace(vm.Driver) && string.IsNullOrWhiteSpace(vm.BreakdownLocation) && vm.ProblemNumber == null)
                return RedirectToAction(nameof(Index)).WithWarning("Please enter at least one field to search on.", "");

            var results = await _workOrderLegacyService.GetInquiryResultList(vm.EquipmentNumber, vm.Route, vm.TransDate, vm.Driver, vm.BreakdownLocation, vm.ProblemNumber, vm.Include);
            vm.Results = results.Select(r => new WorkOrderLegacyInquiryViewModel.InquiryResult
            {
                WorkOrderLegacyId = r.WorkOrderLegacyId,
                EquipmentNumber = r.EquipmentNumber,
                BreakdownLocation = r.BreakdownLocation,
                Route = r.Route,
                TransDate = r.TransDate,
                ResolveDate = r.ResolveDate,
                Driver = r.Driver,
                ProblemNumber = r.ProblemNumber,
                RecType = r.RecType
            });

            return View("Results", vm);
        }
        catch (Exception ex)
        {
            return View("Index", vm).WithDanger(ex.Message, "");
        }
    }
}
