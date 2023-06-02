using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.RouteType;

namespace SW.InternalWeb.Controllers;

public class RouteTypeController : Controller
{
    private readonly IRouteTypeService routeTypeService;

    public RouteTypeController(IRouteTypeService routeTypeService)
    {
        this.routeTypeService = routeTypeService;
    }

    public async Task<IActionResult> Index()
    {
        var types = await routeTypeService.GetAll();
        var model = types.Select(r => new RouteTypeListViewModel
        {
            RouteNumber = r.RouteNumber.ToString(),
            RouteTypeID = r.RouteTypeId,
            Type = r.Type
        }).ToList();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var type = await routeTypeService.GetById(id);
        var model = new RouteTypeEditViewModel
        {
            RouteNumber = type.RouteNumber.ToString(),
            RouteTypeID = type.RouteTypeId,
            Type = type.Type
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(RouteTypeEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View("Edit", vm);

        var routeType = await routeTypeService.GetById(vm.RouteTypeID);
        if (routeType == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "Route type not found");

        int routeNumber = int.Parse(vm.RouteNumber);
        if(routeNumber != routeType.RouteNumber)
        {
            var other = await routeTypeService.GetByRouteNumber(routeNumber);
            if(other != null)
            {
                ModelState.AddModelError(nameof(vm.RouteNumber), $"Duplicate route number {vm.RouteNumber}");
                return View("Edit", vm);
            }
        }

        routeType.ChgDateTime = DateTime.Now;
        routeType.ChgToi = User.GetNameOrEmail();
        routeType.Type = vm.Type.ToUpper();
        routeType.RouteNumber = routeNumber;
        await routeTypeService.Update(routeType);

        return RedirectToAction("Edit", new { id = routeType.RouteTypeId })
            .WithSuccess("", "Route type updated");
    }

    public async Task<IActionResult> Delete(int id)
    {
        var routeType = await routeTypeService.GetById(id);
        if (routeType == null || routeType.DeleteFlag)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "Route type not found");

        routeType.DelDateTime = DateTime.Now;
        routeType.DeleteFlag = true;
        routeType.DelToi = User.GetNameOrEmail();

        await routeTypeService.Update(routeType);

        return RedirectToAction(nameof(Index))
            .WithSuccess("", "Route type deleted");
    }

    public IActionResult Add()
    {
        var model = new RouteTypeAddViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(RouteTypeAddViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var routeNumber = int.Parse(vm.RouteNumber);

        var other = await routeTypeService.GetByRouteNumber(routeNumber);
        if (other != null)
        {
            ModelState.AddModelError(nameof(vm.RouteNumber), $"Duplicate route number {vm.RouteNumber}");
            return View(vm);
        }

        var routeType = new RouteType
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            RouteNumber = routeNumber,
            Type = vm.Type.ToUpper()
        };
        await routeTypeService.Add(routeType);

        return RedirectToAction("Edit", new { id = routeType.RouteTypeId });
    }
}
