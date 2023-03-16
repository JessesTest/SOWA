using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models;
using Common.Extensions;
using PE.BL.Services;
using SW.BLL.Services;

namespace SW.ExternalWeb.ViewComponents
{
    public class LayoutRibbonViewComponent : ViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IPersonEntityService _personEntityService;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LayoutRibbonViewComponent(ICustomerService customerService, IPersonEntityService personEntityService, ITransactionService transactionService, UserManager<ApplicationUser> userManager)
        {
            _customerService = customerService;
            _personEntityService = personEntityService;
            _transactionService = transactionService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string controller, string action)
        {
            var vm = new RibbonViewModel
            {
                CurrentAction = action,
                CurrentController = controller
            };

            var user = await _userManager.FindByIdAsync(UserClaimsPrincipal.GetUserId());
            if (user == null)
                return View("LayoutRibbonBlank", vm);

            var person = await _personEntityService.GetById(user.UserId);
            if (person == null)
                return View("LayoutRibbonBlank", vm);

            var customer = await _customerService.GetByPE(person.Id);
            if (customer == null)
                return View("LayoutRibbonBlank", vm);

            vm.AccountNumber = customer.CustomerId.ToString();
            vm.PastDueBalance = (await _transactionService.GetPastDueAmount(customer.CustomerId)).ToString();
            vm.UserName = person.FullName;

            return View("LayoutRibbon", vm);
        }
    }
}
