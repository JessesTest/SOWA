using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models;

namespace SW.InternalWeb.ViewComponents
{
    public class CustomerAlertsViewComponent : ViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IPersonEntityService _personEntityService;

        public CustomerAlertsViewComponent(ICustomerService customerService, IPersonEntityService personEntityService)
        {
            _customerService = customerService;
            _personEntityService = personEntityService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? customerId)
        {
            var model = new CustomerAlertsViewModel();

            if (!customerId.HasValue)
                return View(model);

            var customer = await _customerService.GetById(customerId.Value);
            if (customer == null)
                return View(model);

            model.PaymentPlanFlag = customer.PaymentPlan;

            var personEntity = await _personEntityService.GetById(customer.Pe);
            if (personEntity == null)
                return View(model);

            model.BadAddressFlag = personEntity.Pab == true;

            return View(model);
        }
    }
}
