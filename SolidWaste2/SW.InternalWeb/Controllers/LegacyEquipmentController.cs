using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.InternalWeb.Models.LegacyEquipment;

namespace SW.InternalWeb.Controllers;

public class LegacyEquipmentController : Controller
{
    private readonly IEquipmentLegacyService _equipmentLegacyService;

    public LegacyEquipmentController(IEquipmentLegacyService equipmentLegacyService)
    {
        _equipmentLegacyService = equipmentLegacyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var equipmentLegacies = await _equipmentLegacyService.GetAll();
        var vm = equipmentLegacies.Select(el => new LegacyEquipmentListViewModel
        {
            EquipmentLegacyID = el.EquipmentLegacyId,
            EquipmentNumber = el.EquipmentNumber,
            EquipmentYear = el.EquipmentYear,
            EquipmentMake = el.EquipmentMake,
            EquipmentModel = el.EquipmentModel,
            EquipmentVIN = el.EquipmentVin
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var equipmentLegacy = await _equipmentLegacyService.GetById(id);
        var vm = new LegacyEquipmentDetailViewModel
        {
            EquipmentLegacyID = equipmentLegacy.EquipmentLegacyId,
            EquipmentNumber = equipmentLegacy.EquipmentNumber,
            EquipmentYear = equipmentLegacy.EquipmentYear,
            EquipmentMake = equipmentLegacy.EquipmentMake,
            EquipmentModel = equipmentLegacy.EquipmentModel,
            EquipmentVIN = equipmentLegacy.EquipmentVin,
            PurchaseDate = equipmentLegacy.PurchaseDate?.ToString("MM/dd/yy"),
            Mileage = equipmentLegacy.Mileage,
            PurchaseAmt = equipmentLegacy.PurchaseAmt?.ToString("c"),
            MonthlyDepreciationAmt = equipmentLegacy.MonthlyDepreciationAmt?.ToString("c"),
            EquipmentDescription = equipmentLegacy.EquipmentDescription,
            DateOfLastPM = equipmentLegacy.DateOfLastPm?.ToString("MM/dd/yy"),
            MilesAtLastPM = equipmentLegacy.MilesAtLastPm,
            StandbyEquipmentFlag = equipmentLegacy.StandbyEquipmentFlag,
            AddDateTime = equipmentLegacy.AddDateTime.ToString("MM/dd/yy"),
            AddToi = equipmentLegacy.AddToi,
            ChgDateTime = equipmentLegacy.ChgDateTime?.ToString("MM/dd/yy"),
            ChgToi = equipmentLegacy.ChgToi
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Search(LegacyEquipmentDetailViewModel vm)
    {
        try
        {
            var equipmentLegacy = await _equipmentLegacyService.GetByEquipmentNumber(vm.EquipmentNumber);
            if (equipmentLegacy == null)
                throw new ApplicationException("Equipment # not found!");

            return RedirectToAction(nameof(Detail), new { id = equipmentLegacy.EquipmentLegacyId });
        }
        catch (Exception ex)
        {
            ModelState.Clear();
            vm = new LegacyEquipmentDetailViewModel{ EquipmentNumber = vm.EquipmentNumber };

            return View("Detail", vm).WithDanger(ex.Message, "");
        }
    }
}
