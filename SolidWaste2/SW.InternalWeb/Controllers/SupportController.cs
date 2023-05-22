using Common.Web.Extensions.Alerts;
using Identity.BL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PE.BL.Services;
using SW.BLL.Services;
using SW.InternalWeb.Identity;
using SW.InternalWeb.Models.Support;

namespace SW.InternalWeb.Controllers;

public class SupportController : Controller
{
    private readonly ICustomerService customerService;
    private readonly IPersonEntityService personService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IConfiguration configuration;
    private readonly IUserNotificationService notificationService;

    public SupportController(
        ICustomerService customerService,
        IPersonEntityService personService,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IUserNotificationService notificationService)
    {
        this.customerService = customerService;
        this.personService = personService;
        this.userManager = userManager;
        this.configuration = configuration;
        this.notificationService = notificationService;
    }

    public async Task<IActionResult> Index(int? customerId)
    {
        if(customerId == null)
        {
            return View(new SupportViewModel());
        }

        var customer = await customerService.GetById(customerId.Value);
        if(customer == null)
        {
            return View(new SupportViewModel())
                .WithWarning("", $"Customer id {customerId} is invalid");
        }

        var person = await personService.GetById(customer.Pe);
        if(person == null)
        {
            return View(new SupportViewModel())
                .WithWarning("", $"Person id {customer.Pe} is invalid");
        }

        var user = await userManager.Users.SingleOrDefaultAsync(u => u.UserId == person.Id);

        SupportViewModel model = new()
        {
            CustomerId = customer.CustomerId,
            PersonEntityId = person.Id,
            CustomerName = person.FullName,
            HasOnline = user != null,
            EmailConfirmationFlag = user?.EmailConfirmed ?? false,
            Email = user?.Email,
            Password = null
            //ConfirmPassword = null
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResendConfirmationEmail(SupportViewModel vm)
    {
        if (vm == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "Form invalid");

        if (vm.PersonEntityId == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "PersonEntityId invalid");

        var person = await personService.GetById(vm.PersonEntityId.Value);
        if(person == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "PersonEntityId invalid");

        var user = await userManager.Users.SingleOrDefaultAsync(u => u.UserId == person.Id);
        if (user == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "UserId is invalid");

        if (user.EmailConfirmed)
            return RedirectToAction(nameof(Index), new { vm.CustomerId })
                .WithInfo("", "Email is already confirmed");

        var externalUri = new Uri(configuration["ExternalWebsite"]);
        var callbackUri = new Uri(externalUri, "/Account/ConfirmEmail");

        await notificationService.SendConfirmationEmailByUserId(user.UserId, callbackUri.ToString());

        return RedirectToAction(nameof(Index), new { vm.CustomerId })
            .WithSuccess("success", "Confirmation email sent to " + user.Email);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(SupportViewModel vm)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "Form invalid");

        if (vm.PersonEntityId == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "PersonEntityId invalid");

        var person = await personService.GetById(vm.PersonEntityId.Value);
        if (person == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "PersonEntityId invalid");

        var user = await userManager.Users.SingleOrDefaultAsync(u => u.UserId == person.Id);
        if (user == null)
            return RedirectToAction(nameof(Index))
                .WithWarning("", "UserId is invalid");

        foreach(var validator in userManager.PasswordValidators)
        {
            var validateResult = await validator.ValidateAsync(userManager, user, vm.Password);
            if (!validateResult.Succeeded)
            {
                var errorMessage = validateResult.Errors.First().Description;
                ModelState.AddModelError(nameof(vm.Password), errorMessage);
                return View(nameof(Index), vm)
                    .WithWarning("Invalid Password", errorMessage);
            }
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, resetToken, vm.Password);
        if (!resetResult.Succeeded)
        {
            return View(nameof(Index), vm)
                .WithWarning("", resetResult.Errors.First().Description);
        }

        return View(nameof(Index), vm)
            .WithSuccess("", "Password reset");
    }
}
