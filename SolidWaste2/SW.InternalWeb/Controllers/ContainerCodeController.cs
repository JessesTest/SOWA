using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.ContainerCodes;

namespace SW.InternalWeb.Controllers;

public class ContainerCodeController : Controller
{
    private readonly IContainerCodeService containerCodeService;

    public ContainerCodeController(IContainerCodeService containerCodeService)
    {
        this.containerCodeService = containerCodeService;
    }

    public async Task<IActionResult> Index()
    {
        var temp = await containerCodeService.GetAll();
        var vm = temp.Select(cc => new ContainerCodeListViewModel
        {
            //BillingFrequencies = cc.BillingFrequency,
            ContainerCodeID = cc.ContainerCodeId,
            Description = cc.Description,
            Type = cc.Type
        })
            .ToList();

        return View(vm);
    }

    #region Add

    [HttpGet]
    public IActionResult Add()
    {
        return View(new ContainerCodeAddViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Add(ContainerCodeAddViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        ContainerCode containerCode = new()
        {
            AddDateTime = DateTime.Now,
            AddToi = User.GetNameOrEmail(),
            //BillingFrequency = null,
            //ChgDateTime = null,
            //ChgToi = null,
            //ContainerCodeId
            //ContainerRates = Array.Empty<ContainerRate>(),
            //Containers = Array.Empty<Container>(),
            //ContainerSubtypes = Array.Empty<ContainerSubtype>(),
            //DelDateTime = null,
            //DeleteFlag = false,
            //DelToi = null,
            Description = model.Description,
            Type = model.Type
        };

        await containerCodeService.Add(containerCode);

        ModelState.Clear();
        ModelState.AddModelError("success", "Add Successful");
        return RedirectToAction("Edit", new { id = containerCode.ContainerCodeId });
    }

    #endregion

    #region Edit

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var containerCode = await containerCodeService.GetById(id);
        if (containerCode == null || containerCode.DeleteFlag)
        {
            ModelState.AddModelError("exception", "Invalid container code");
            return RedirectToAction(nameof(Index));
        }

        var vm = new ContainerCodeEditViewModel
        {
            ContainerCodeID = containerCode.ContainerCodeId,
            Description = containerCode.Description,
            Type = containerCode.Type
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ContainerCodeEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var containerCode = await containerCodeService.GetById(vm.ContainerCodeID);
        if (containerCode == null || containerCode.DeleteFlag)
        {
            ModelState.AddModelError("exception", "Invalid container code");
            return RedirectToAction(nameof(Index));
        }

        var username = User.GetNameOrEmail();

        containerCode.Description = vm.Description;
        containerCode.Type = vm.Type;
        containerCode.ChgDateTime = DateTime.Now;
        containerCode.ChgToi = username;

        await containerCodeService.Update(containerCode);

        ModelState.Clear();
        ModelState.AddModelError("success", "Update Successful");
        return RedirectToAction("Edit", new { id = vm.ContainerCodeID });
    }

    #endregion

    #region Delete

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var containerCode = await containerCodeService.GetById(id);
        if (containerCode == null || containerCode.DeleteFlag)
        {
            ModelState.AddModelError("exception", "Invalid container code");
            return RedirectToAction(nameof(Index));
        }

        containerCode.DelDateTime = DateTime.Now;
        containerCode.DeleteFlag = true;
        containerCode.DelToi = User.GetNameOrEmail();
        await containerCodeService.Delete(containerCode);

        return RedirectToAction(nameof(Index));
    }

    #endregion
}
