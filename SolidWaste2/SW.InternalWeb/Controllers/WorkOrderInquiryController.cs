using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.InternalWeb.Models.WorkOrderInquiry;

namespace SW.InternalWeb.Controllers;

public class WorkOrderInquiryController : Controller
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrderInquiryController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var vm = new WorkOrderInquiryViewModel
        {
            Include = false
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Results(WorkOrderInquiryViewModel vm)
    {
        try
        {
            if (vm.WorkOrderId == null && string.IsNullOrWhiteSpace(vm.ContainerRoute) && !vm.TransDate.HasValue && string.IsNullOrWhiteSpace(vm.DriverInitials) && string.IsNullOrWhiteSpace(vm.CustomerName) && string.IsNullOrWhiteSpace(vm.CustomerAddress))
                return RedirectToAction(nameof(Index)).WithWarning("Please enter at least one field to search on.", "");

            var results = await _workOrderService.GetInquiryResultList(vm.WorkOrderId, vm.ContainerRoute, vm.TransDate, vm.DriverInitials, vm.CustomerName, vm.CustomerAddress, vm.Include);
            vm.Results = results.Select(r => new WorkOrderInquiryViewModel.InquiryResult
            {
                WorkOrderId = r.WorkOrderId,
                CustomerName = r.CustomerName,
                CustomerAddress = r.CustomerAddress,
                //CustomerId = r.CustomerId,
                ContainerRoute = r.ContainerRoute,
                TransDate = r.TransDate,
                ResolveDate = r.ResolveDate,
                DriverInitials = r.DriverInitials
            });

            return View("Results", vm);
        }
        catch (Exception ex)
        {
            return View("Index", vm).WithDanger(ex.Message, "");
        }
    }
}
