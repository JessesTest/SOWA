using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.WorkOrder;
using System.ComponentModel;

namespace SW.InternalWeb.Controllers;

public class WorkOrderController : Controller
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrderController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? workOrderId)
    {
        //WorkOrderViewModel vm = new WorkOrderViewModel();
        var vm = new WorkOrderViewModel();

        try
        {
            if (workOrderId.HasValue)
            {
                var workOrder = await _workOrderService.GetById(workOrderId.Value);
                if (workOrder == null)
                    throw new ArgumentException("[WorkOrderId] invalid");

                vm.WorkOrderId = workOrder.WorkOrderId;
                vm.TransDate = workOrder.TransDate?.ToString("MM/dd/yyyy");
                vm.ResolveDate = workOrder.ResolveDate?.ToString("MM/dd/yyyy");
                vm.DriverInitials = workOrder.DriverInitials;
                vm.CustomerId = workOrder.CustomerId;
                vm.CustomerType = workOrder.CustomerType;
                vm.CustomerName = workOrder.CustomerName;
                vm.CustomerAddress = workOrder.CustomerAddress;
                vm.ServiceAddressId = workOrder.ServiceAddressId;
                vm.ContainerId = workOrder.ContainerId;
                vm.ContainerCode = workOrder.ContainerCode;
                vm.RecyclingFlag = workOrder.RecyclingFlag;
                vm.ContainerRoute = workOrder.ContainerRoute;
                vm.ContainerSize = workOrder.ContainerSize;
                vm.ContainerPickupMon = workOrder.ContainerPickupMon;
                vm.ContainerPickupTue = workOrder.ContainerPickupTue;
                vm.ContainerPickupWed = workOrder.ContainerPickupWed;
                vm.ContainerPickupThu = workOrder.ContainerPickupThu;
                vm.ContainerPickupFri = workOrder.ContainerPickupFri;
                vm.ContainerPickupSat = workOrder.ContainerPickupSat;
                vm.RepairsNeeded = workOrder.RepairsNeeded;
                vm.ResolutionNotes = workOrder.ResolutionNotes;
                vm.AddToi = workOrder.AddToi;
                vm.AddDateTime = workOrder.AddDateTime?.ToString("MM/dd/yyyy hh:mm:ss tt");
                vm.ChgToi = workOrder.ChgToi;
                vm.ChgDateTime = workOrder.ChgDateTime?.ToString("MM/dd/yyyy hh:mm:ss tt");
            }

            //if (!workOrderId.HasValue)
            //    vm = new WorkOrderViewModel();
            //else
            //{
            //    SW_BL.BusinessLayer swbl = new SW_BL.BusinessLayer();
            //    SW_DM.WorkOrder workOrder = swbl.GetWorkOrderById(workOrderId.Value);
            //    Mapper.Map<SW_DM.WorkOrder, WorkOrderViewModel>(workOrder, vm);
            //}

            return View(vm);
        }
        catch (Exception ex)
        {
            return View(vm).WithDanger(ex.Message, "");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Add(WorkOrderViewModel vm)
    {
        try
        {
            var workOrder = new WorkOrder
            {
                //WorkOrderId = vm.WorkOrderId.Value,
                //TransDate = vm.TransDate,
                ResolveDate = string.IsNullOrWhiteSpace(vm.ResolveDate) ? null : DateTime.Parse(vm.ResolveDate),
                DriverInitials = vm.DriverInitials,
                CustomerId = vm.CustomerId,
                CustomerType = vm.CustomerType,
                CustomerName = vm.CustomerName,
                CustomerAddress = vm.CustomerAddress,
                ServiceAddressId = vm.ServiceAddressId,
                ContainerId = vm.ContainerId,
                ContainerCode = vm.ContainerCode,
                RecyclingFlag = vm.RecyclingFlag,
                ContainerRoute = vm.ContainerRoute,
                ContainerSize = vm.ContainerSize,
                ContainerPickupMon = vm.ContainerPickupMon,
                ContainerPickupTue = vm.ContainerPickupTue,
                ContainerPickupWed = vm.ContainerPickupWed,
                ContainerPickupThu = vm.ContainerPickupThu,
                ContainerPickupFri = vm.ContainerPickupFri,
                ContainerPickupSat = vm.ContainerPickupSat,
                RepairsNeeded = vm.RepairsNeeded,
                ResolutionNotes = vm.ResolutionNotes,

                AddToi = User.GetNameOrEmail()
                //AddDateTime = vm.AddDateTime,
                //ChgToi = vm.ChgToi,
                //ChgDateTime = vm.ChgDateTime
            };

            await _workOrderService.Add(workOrder);

            return RedirectToAction(nameof(Index), new { workOrderId = workOrder.WorkOrderId }).WithSuccess("Work Order successfully created", "");

            //SW_DM.WorkOrder workOrder = Mapper.Map<SW_DM.WorkOrder>(vm);
            //SW_BL.BusinessLayer swbl = new SW_BL.BusinessLayer();
            //swbl.AddWorkOrder(workOrder, User.Identity.Name);

            //ModelState.Clear();
            //ModelState.AddModelError("success", "Work Order successfully created");
            //return RedirectToAction("Index", "WorkOrder", new { workOrderId = workOrder.WorkOrderId });
        }
        catch (Exception ex)
        {
            return View("Index", vm).WithDanger(ex.Message, "");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(WorkOrderViewModel vm)
    {
        try
        {
            var workOrder = await _workOrderService.GetById(vm.WorkOrderId.Value);
            if (workOrder == null)
                throw new ArgumentException("[WorkOrderId] invalid");

            //WorkOrderId = vm.WorkOrderId.Value,
            //TransDate = vm.TransDate,
            workOrder.ResolveDate = string.IsNullOrWhiteSpace(vm.ResolveDate) ? null : DateTime.Parse(vm.ResolveDate);
            workOrder.DriverInitials = vm.DriverInitials;
            workOrder.CustomerId = vm.CustomerId;
            workOrder.CustomerType = vm.CustomerType;
            workOrder.CustomerName = vm.CustomerName;
            workOrder.CustomerAddress = vm.CustomerAddress;
            workOrder.ServiceAddressId = vm.ServiceAddressId;
            workOrder.ContainerId = vm.ContainerId;
            workOrder.ContainerCode = vm.ContainerCode;
            workOrder.RecyclingFlag = vm.RecyclingFlag;
            workOrder.ContainerRoute = vm.ContainerRoute;
            workOrder.ContainerSize = vm.ContainerSize;
            workOrder.ContainerPickupMon = vm.ContainerPickupMon;
            workOrder.ContainerPickupTue = vm.ContainerPickupTue;
            workOrder.ContainerPickupWed = vm.ContainerPickupWed;
            workOrder.ContainerPickupThu = vm.ContainerPickupThu;
            workOrder.ContainerPickupFri = vm.ContainerPickupFri;
            workOrder.ContainerPickupSat = vm.ContainerPickupSat;
            workOrder.RepairsNeeded = vm.RepairsNeeded;
            workOrder.ResolutionNotes = vm.ResolutionNotes;

            //workOrder.AddToi = User.GetNameOrEmail();
            //AddDateTime = vm.AddDateTime,
            workOrder.ChgToi = User.GetNameOrEmail();
            //ChgDateTime = vm.ChgDateTime

            await _workOrderService.Update(workOrder);

            return RedirectToAction(nameof(Index), new { workOrderId = workOrder.WorkOrderId }).WithSuccess("Work Order successfully updated", "");

            //SW_DM.WorkOrder workOrder = Mapper.Map<SW_DM.WorkOrder>(vm);
            //SW_BL.BusinessLayer swbl = new SW_BL.BusinessLayer();
            //swbl.UpdateWorkOrder(workOrder, User.Identity.Name);

            //ModelState.Clear();
            //ModelState.AddModelError("success", "Work Order successfully updated");
            //return RedirectToAction("Index", "WorkOrder", new { workOrderId = workOrder.WorkOrderId });
        }
        catch (Exception ex)
        {
            return View("Index", vm).WithDanger(ex.Message, "");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int workOrderId)
    {
        try
        {
            var workOrder = await _workOrderService.GetById(workOrderId);
            if (workOrder == null)
                throw new ArgumentException("[WorkOrderId] invalid");

            workOrder.DelToi = User.GetNameOrEmail();

            await _workOrderService.Delete(workOrder);

            return RedirectToAction("Index", "WorkOrderInquiry").WithSuccess("Work Order successfully deleted", "");

            //SW_BL.BusinessLayer swbl = new SW_BL.BusinessLayer();
            //SW_DM.WorkOrder workOrder = swbl.GetWorkOrderById(workOrderId);
            //swbl.DeleteWorkOrder(workOrder, User.Identity.Name);

            //ModelState.Clear();
            //ModelState.AddModelError("success", "Work Order successfully deleted");
            //return RedirectToAction("Index", "WorkOrderInquiry");
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Index), new { workOrderId }).WithDanger(ex.Message, "");
        }
    }
}
