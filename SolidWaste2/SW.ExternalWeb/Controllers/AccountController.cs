using Common.Services.Email;
using Common.Web.Extensions.Alerts;
using Identity.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using SW.BLL.Services;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.Account;
using System.Text;

namespace SW.ExternalWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IPersonEntityService personEntityService;
        private readonly IUserService userService;
        private readonly IUserNotificationService userNotificationService;
        private readonly ICustomerService customerService;
        private readonly ISendGridService sendGridService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPersonEntityService personEntityService,
            IUserService userService,
            IUserNotificationService userNotificationService,
            ICustomerService customerService,
            ISendGridService sendGridService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.personEntityService = personEntityService;
            this.userService = userService;
            this.userNotificationService = userNotificationService;
            this.customerService = customerService;
            this.sendGridService = sendGridService;
        }

        #region Login & 2FA

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User != null && User.Identity.IsAuthenticated)
                return RedirectToAction("BillSummary", "Home");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
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

                    return View(model).WithDanger("You must have a confirmed email to log on.", "");
                }
            }
            else
            {
                return View(model).WithDanger("Incorrect username and/or password combination", "");
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
                return RedirectToAction(nameof(SendCode), new { UserId = user.Id });
            }
            else
            {
                return View(model).WithDanger("Incorrect username and/or password combination", "");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendCode()
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Login))
                    .WithWarning("Account", "Not a two factor user");

            var userFactors = await userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Login))
                    .WithWarning("Account", "Not a two factor user");

            if (!ModelState.IsValid)
            {
                var userFactors = await userManager.GetValidTwoFactorProvidersAsync(user);
                var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
                model.Providers = factorOptions;
                return View(model);
            }

            var token = await userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);

            if(model.SelectedProvider == "Email")
            {
                await userNotificationService.SendEmail2FA(user.Email, token);
                return RedirectToAction("VerifyCode", model);
            }

            return View(model).WithDanger($"Invalid provider {model.SelectedProvider}", "");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string selectedProvider, string returnUrl)
        {
            var twoFAUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (twoFAUser == null)
                return RedirectToAction(nameof(Login))
                    .WithWarning("Account", "Not a two factor user");

            return View(new VerifyCodeViewModel { Provider = selectedProvider, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await signInManager.TwoFactorSignInAsync(model.Provider, model.Code, false, model.RememberBrowser);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "~/");
            }
            else if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                return View(model).WithDanger("Invalid code.", "");
            }
        }

        #endregion

        #region Register

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            string systemCode = model.Code.Substring(0, 2).ToUpper();
            if (!int.TryParse(model.Code.Substring(2), out int account))
                return View(model).WithDanger("PIN is not in the correct format.", "");

            var pe = await personEntityService.GetBySystemAndCode(systemCode, account);
            if (pe == null)
                return View(model).WithDanger("PIN does not exist or name does not match", "");
            if (pe.FullName.ToUpper().Trim() != model.FullName.ToUpper().Trim())
                return View(model).WithDanger("Name on Bill field is CASE SENSITIVE and must match both the case and spelling on the bill.", "");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName?.ToUpper().Trim(),
                    Email = model.Email?.ToLower().Trim(),
                    UserId = pe.Id ,
                    NormalizedEmail = model.Email?.ToUpper().Trim(),
                    NormalizedUserName = model.UserName?.ToUpper().Trim()
                };
                var userList = await userManager.Users.Where(u => u.UserId == pe.Id).ToListAsync();

                if (userList.Any())
                {
                    return View(model).WithDanger("There is already a user registered for this account", "");
                }
                else
                {
                    var result = await userManager.CreateAsync(user, model.Password);
                    if (result.Errors.Any())
                        return View().WithDanger(result.Errors.First().Description, "");
                    if (result.Succeeded)
                    {
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", null, "https");
                        _ = userNotificationService.SendConfirmationEmailByUserId(user.UserId, callbackUrl);

                        return RedirectToAction("Login", "Account").WithSuccess("Thank you for registering. You will need to confirm your email before logging in.", "");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(int userId, string code)
        {
            var result = await userService.ConfirmEmail(userId, code);

            return View(result ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendEmailConfirmation()
        {
            var model = new EmailConfirmationViewModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(EmailConfirmationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(nameof(ResendEmailConfirmation), model);

                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null || user.EmailConfirmed)
                    return View(model).WithDanger($"Unconfimed email {model.Email} was not found.", "");

                var callbackUrl = Url.Action("ConfirmEmail", "Account", null, "https");
                _ = userNotificationService.SendConfirmationEmailByUserId(user.UserId, callbackUrl);

                return RedirectToAction("Login", "Account").WithSuccess("You will need to confirm your email before logging in.", "");
            }
            catch (Exception e)
            {
                return View(nameof(ResendEmailConfirmation), model).WithDanger(e.Message, "");
            }
        }

        #endregion

        #region Password Reset

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if(!ModelState.IsValid)
                return View("ForgotPasswordConfirmation");

            var users = await userService.FindAllByEmail(model.Email);

            if (!users.Any())
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            var userPotatoes = new List<ResetPasswordPotato>();
            foreach (var aspnetuser in users)
            {
                var identityUser = await userManager.FindByIdAsync(aspnetuser.Id);
                var code = await userManager.GeneratePasswordResetTokenAsync(identityUser);
                var customer = await customerService.GetByPE(aspnetuser.UserId);
                userPotatoes.Add(new ResetPasswordPotato()
                {
                    Url = Url.Action("ResetPassword", "Account", new { userId = aspnetuser.Id, code }, "https"),
                    AccountNumber = customer.CustomerId.ToString(),
                    UserName = aspnetuser.UserName
                });
            }
            var sb = new StringBuilder(4096);
            sb.AppendLine("<div style=\"background-color:#909EB8\">");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("         <div style=\"background-color:#344479; min-height:100px; padding-left: 20px; padding-right: 20px; padding-top: 28px; height: 100px; min-width: 700px; display:table; overflow:hidden;\">");
            sb.AppendLine("             <div style=\"display: table-cell; vertical-align: middle; font-size: 40px; color:white; font-family: Verdana;\">");
            sb.AppendLine("                 <div align=\"center\">");
            sb.AppendLine("                     Shawnee County Solid Waste");
            sb.AppendLine("                 </div>");
            sb.AppendLine("             </div>");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("        <div style=\"padding: 20px 20px 20px 20px; background-color: #D4D4D4; width: 700px;\">");
            sb.AppendLine("            <div style=\"padding: 20px 20px 20px 20px; background-color: #FFFFFF;\">");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px;\">");
            sb.AppendLine("                    Your account has requested a password reset. Please click on the link below to reset your password:");
            sb.AppendLine("                </div>");
            sb.AppendLine("<div width=\"600px\">");
            sb.AppendLine("<table border=\"1|0\" width=\"500px\" align=\"center\" cellpadding=\"5\" bordercolor=\"#344479\">");
            sb.AppendLine("<th bgcolor=\"#6495ED\">");
            sb.AppendLine("Account Number");
            sb.AppendLine("</th>");
            sb.AppendLine("<th bgcolor=\"#6495ED\">");
            sb.AppendLine("User Name");
            sb.AppendLine("</th>");
            sb.AppendLine("<th bgcolor=\"#6495ED\">");
            sb.AppendLine("");
            sb.AppendLine("</th>");
            // Once you get to the part that shows the detail for each user, start using the foreach loop
            foreach (var p in userPotatoes)
            {

                sb.AppendLine("<tr align=\"center\">");
                sb.AppendLine("<td bgcolor=\"#6495ED\">");
                sb.AppendLine(p.AccountNumber);
                sb.AppendLine("</td>");
                sb.AppendLine("<td bgcolor=\"#6495ED\">");
                sb.AppendLine(p.UserName);
                sb.AppendLine("</td>");
                sb.AppendLine("<td bgcolor=\"#6495ED\">");
                sb.AppendLine("<div style=\"text-align=center; padding-top:10px; padding-bottom:10px\">");
                sb.AppendLine("<div align=\"center\">");
                sb.AppendLine($"<a style=\"padding: 8px 15px 8px 15px; text-align: center; background-color: #6EBF4A; color: #FFFFFF; font-weight:bold; text-decordation: none;\" href=\"{p.Url}\">");
                sb.AppendLine("Reset this Account");
                sb.AppendLine("</a>");
                sb.AppendLine("</div>");
                sb.AppendLine("</div>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                // Show a line for each user in the list that displays their Username, AccountNumber, and a link to reset that account
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("                 </div>");
            sb.AppendLine("         <div align=\"center\" style=\"padding-top:20px\">");
            sb.AppendLine("             <div style=\"font-size:12px;\">");
            sb.AppendLine("                 DO NOT REPLY TO THIS EMAIL. If you've received this email in error, please notify us by telephone at 785.233.4774 or by email at solidwaste@snco.us.");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("<hr>");
            sb.AppendLine("     <div width=\"700px\">");
            sb.AppendLine("         Visit the Shawnee County Website at http://www.snco.us");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div width=\"700px\">");
            sb.AppendLine("         Questions? Contact Solid Waste at 785.233.4774 (voice) 785.291.4928(fax)");
            sb.AppendLine("     </div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            var email = new SendEmailDto
            {
                HtmlContent = sb.ToString(),
                Subject = "Reset Your Password",
            }
            .SetFrom("no-reply.scsw@sncoapps.us")
            .AddTo(model.Email);

            _ = sendGridService.SendSingleEmail(email);

            return View("ForgotPasswordConfirmation");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            if (code == null || userId == null)
                return View("Error");

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return View("Error");

            var vm = new ResetPasswordViewModel
            {
                UserId = user.Id,
                Code = code
            };

            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password); 
            if (result.Errors.Any())
                return View().WithDanger(result.Errors.First().Description, "");
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation", "Account");

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Logoff

        [HttpGet]
        public async Task<ActionResult> LogOff()
        {
            if (User.Identity.IsAuthenticated)
            {
                await signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}

