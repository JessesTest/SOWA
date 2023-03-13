using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models;
using Common.Extensions;

namespace SW.ExternalWeb.ViewComponents
{
    public class LayoutRibbonViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LayoutRibbonViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string controller, string action)
        {
            var vm = new RibbonViewModel();

            var user = _userManager.FindByIdAsync(UserClaimsPrincipal.GetUserId());
            if (user == null)
            {
                vm.CurrentController = controller;
                vm.CurrentAction = action;
                return View("LayoutRibbonBlank", vm);
            }

            // needs to be updated with new business logic
            //PE.BL.BusinessLayer pebl = new PE.BL.BusinessLayer();
            //PE.DM.PersonEntity person = pebl.GetPersonEntityById(user.UserId);
            //SW_BL.BusinessLayer swbl = new SW_BL.BusinessLayer();
            //SW_DM.Customer customer = swbl.GetCustomerByPE(person.Id);
            //vm.AccountNumber = customer.CustomerID.ToString();
            //vm.PastDueBalance = swbl.GetPastDueAmount(customer.CustomerID).ToString();
            //vm.UserName = person.FullName;
            vm.CurrentController = controller;
            vm.CurrentAction = action;

            return View("LayoutRibbon", vm);
        }
    }
}
