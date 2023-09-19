using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SW.InternalWeb.Identity;

namespace SW.InternalWeb.Controllers;

public class TestController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;

    public TestController(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users.ToListAsync();
        return View(users);
    }
}
