using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.InternalWeb.Models.Legacy;

namespace SW.InternalWeb.Controllers
{
    public class LegacyController : Controller
    {
        private readonly IEquipmentLegacyService _equipmentLegacyService;
        private readonly IWorkOrderLegacyService _workOrderLegacyService;

        public LegacyController(
            IEquipmentLegacyService equipmentLegacyService,
            IWorkOrderLegacyService workOrderLegacyService)
        {
            _equipmentLegacyService = equipmentLegacyService;
            _workOrderLegacyService = workOrderLegacyService;
        }

        [HttpGet]
        public async Task<IActionResult> Breakdown(int id)
        {
            var b = await _workOrderLegacyService.GetById(id);
            if (b == null || b.DelFlag)
                return NotFound();

            var vm = new LegacyBViewModel
            {
                AddDate = b.AddDateTime,
                AddToi = b.AddToi,
                BreakdownCode = b.BreakdownCode,
                BreakdownDescription = b.ProblemDescription,
                BreakdownLocation = b.BreakdownLocation,
                ChgDate = b.ChgDateTime,
                ChgToi = b.ChgToi,
                Driver = b.Driver,
                EquipmentNumber = b.EquipmentNumber,
                Mileage = b.Mileage,
                ProblemNumber = b.ProblemNumber,
                ReplaceEuipmentNumber = b.ReplacementEquipmentNumber,
                ResolveDate = b.ResolveDate,
                Route = b.Route,
                TransDate = b.TransDate
            };

            var el = await _equipmentLegacyService.GetByEquipmentNumber(vm.EquipmentNumber);
            if (el != null)
                vm.LegacyEquipmentId = el.EquipmentLegacyId;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> WorkOrder(int id)
        {
            var r = await _workOrderLegacyService.GetById(id);
            if (r == null || r.DelFlag)
                return NotFound();

            var vm = new LegacyRViewModel
            {
                AddDate = r.AddDateTime,
                AddToi = r.AddToi,
                ChgDate = r.ChgDateTime,
                ChgToi = r.ChgToi,
                Driver = r.Driver,
                Mileage = r.Mileage,
                ProblemDescription = r.ProblemDescription,
                ProblemNumber = r.ProblemNumber,
                ResolveDate = r.ResolveDate,
                AlreadyResolvedFlag = r.ResolveDate.HasValue,
                Route = r.Route,
                TransDate = r.TransDate,
                WorkOrderLegacyId = r.WorkOrderLegacyId
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> WorkOrder(LegacyRViewModel model)
        {
            try
            {
                var r = await _workOrderLegacyService.GetById(model.WorkOrderLegacyId);
                if (r == null || r.DelFlag)
                    return NotFound();

                r.ChgToi = User.GetNameOrEmail();
                r.ProblemDescription = model.ProblemDescription;
                r.ResolveDate = model.ResolveDate;

                await _workOrderLegacyService.Update(r);
                return RedirectToAction(nameof(WorkOrder)).WithSuccess("Update Successful", "");
            }
            catch (Exception e)
            {
                return View(model).WithDanger(e.Message, "");
            }
        }
    }
}
