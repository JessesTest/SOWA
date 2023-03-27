using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.Account;

namespace SW.ExternalWeb.Controllers;

[Authorize(Roles = "Admin")]
public class UsersAdminController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;

    public UsersAdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = await userManager.Users.ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
            return BadRequest();

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        ViewBag.RoleNames = await userManager.GetRolesAsync(user);

        return View(user);
    }

    #region Create

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.RoleId = new SelectList(await roleManager.Roles.ToListAsync(), "Name", "Name");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(RegisterViewModel model, params string[] selectedRoles)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoleId = new SelectList(await roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var adminresult = await userManager.CreateAsync(user, model.Password);

        //Add User to the selected Roles 
        if (adminresult.Succeeded)
        {
            if (selectedRoles != null)
            {
                var result = await userManager.AddToRolesAsync(user, selectedRoles);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First().Description);
                    ViewBag.RoleId = new SelectList(await roleManager.Roles.ToListAsync(), "Name", "Name");
                    return View(model);
                }
            }
        }
        else
        {
            ModelState.AddModelError("", adminresult.Errors.First().Description);
            ViewBag.RoleId = new SelectList(roleManager.Roles, "Name", "Name");
            return View(model);

        }
        return RedirectToAction("Index");
    }

    #endregion

    #region Edit

    [HttpGet]
    public async Task<ActionResult> Edit(string id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await userManager.GetRolesAsync(user);

        return View(new EditUserViewModel()
        {
            Id = user.Id,
            Email = user.Email,
            RolesList = (await roleManager.Roles.ToListAsync())
                .Select(r => new SelectListItem(r.Name, r.Name, userRoles.Contains(r.Name)))
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel editUser, params string[] selectedRole)
    {
        var user = await userManager.FindByIdAsync(editUser?.Id);
        if (user == null)
            return NotFound();

        selectedRole ??= Array.Empty<string>();

        editUser.RolesList = (await roleManager.Roles.ToListAsync())
                .Select(r => new SelectListItem(r.Name, r.Name, selectedRole.Contains(r.Name)));

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Something failed.");
            return View(editUser);
        }

        var userRoles = await userManager.GetRolesAsync(user);

        user.UserName = editUser.Email;
        user.Email = editUser.Email;

        var addResult = await userManager.AddToRolesAsync(user, selectedRole.Except(userRoles).ToArray());
        if (!addResult.Succeeded)
        {
            ModelState.AddModelError("", addResult.Errors.First().Description);
            return View(editUser);
        }

        var removeResult = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRole).ToArray());
        if (!removeResult.Succeeded)
        {
            ModelState.AddModelError("", removeResult.Errors.First().Description);
            return View(editUser);
        }

        return RedirectToAction("Index");
    }

    #endregion

    #region Delete

    [HttpGet]
    public async Task<ActionResult> Delete(string id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Errors.First().Description);
            return View(user);
        }
        return RedirectToAction("Index");
    }

    #endregion
}
