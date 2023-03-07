using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.Account;

namespace SW.ExternalWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        #region Login

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User != null && User.Identity.IsAuthenticated)
                return RedirectToAction("BillSummary", "Home");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Require the user to have a confirmed email before they can log on.
            var user = await userManager.FindByNameAsync(model.UserName?.ToUpper());
            if (user != null)
            {
                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    model.EmailIsUnconfirmed = true;
                    ModelState.AddModelError("", "You must have a confirmed email to log on.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Incorrect username and/or password combination");
                return View(model);
            }

            // This doesn't count login failures towards lockout only two factor authentication
            // To enable password failures to trigger lockout, change to shouldLockout: true
            var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if (result.Succeeded)
            {
                return RedirectToAction("BillSummary", "Home");
            }
            else if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode");
            }
            else
            {
                ModelState.AddModelError("", "Incorrect username and/or password combination");
                return View(model);
            }
        }

        #endregion
    }
}
