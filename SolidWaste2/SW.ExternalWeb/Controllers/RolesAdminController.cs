using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.Roles;

namespace SW.ExternalWeb.Controllers;

[Authorize(Roles = "Admin")]
public class RolesAdminController : Controller
{
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<ApplicationUser> userManager;

    public RolesAdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return View(await roleManager.Roles.ToListAsync());
    }

    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var role = await roleManager.FindByIdAsync(id);
        // Get the list of Users in this Role
        var users = new List<ApplicationUser>();

        // Get the list of Users in this Role
        foreach (var user in userManager.Users.ToList())
        {
            if (await userManager.IsInRoleAsync(user, role.Name))
            {
                users.Add(user);
            }
        }

        ViewBag.Users = users;
        ViewBag.UserCount = users.Count;
        return View(role);
    }

    #region Create

    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoleViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleViewModel roleViewModel)
    {
        if (ModelState.IsValid)
        {
            var role = new IdentityRole(roleViewModel.Name);
            var roleresult = await roleManager.CreateAsync(role);
            if (roleresult.Errors.Any())
                return View().WithDanger(roleresult.Errors.First().Description, "");
            if (roleresult.Succeeded)
                return RedirectToAction("Index");
        }
        return View();
    }

    #endregion

    #region Edit

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        var roleModel = new RoleViewModel { Id = role.Id, Name = role.Name };
        return View(roleModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleViewModel roleModel)
    {
        if (ModelState.IsValid)
        {
            var role = await roleManager.FindByIdAsync(roleModel.Id);
            role.Name = roleModel.Name;
            await roleManager.UpdateAsync(role);
            return RedirectToAction("Index");
        }
        return View();
    }

    #endregion

    #region Delete

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        return View(role);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, string deleteUser)
    {
        if (ModelState.IsValid)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            IdentityResult result;
            if (deleteUser != null)
            {
                result = await roleManager.DeleteAsync(role);
            }
            else
            {
                result = await roleManager.DeleteAsync(role);
            }
            if (result.Errors.Any())
                return View().WithDanger(result.Errors.First().Description, "");
            if (result.Succeeded)
                return RedirectToAction("Index");
        }
        return View();
    }

    #endregion
}
