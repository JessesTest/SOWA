using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SW.ExternalWeb.Identity;

namespace SW.ExternalWeb.Controllers;

public class TestController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public TestController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Identity(string email)
    {
        var user = await userManager.FindByNameAsync(email ?? "lee.sykes@snco.us");
        return View(user);
    }

    public async Task<IActionResult> Login()
    {
        var userName = "lee.sykes@snco.us";
        var user = await userManager.FindByNameAsync(userName);

        var p = "_not_the_password_";
        var result = await signInManager.PasswordSignInAsync(user, p, false, false);

        return View(result);
    }
}
