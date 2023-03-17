using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;

namespace SW.ExternalWeb.Controllers;

public class BillingHistoryController : Controller
{
    private readonly IPersonEntityService personEntityService;
    private readonly ICustomerService customerService;

    public BillingHistoryController(IPersonEntityService personEntityService, ICustomerService customerService)
    {
        this.personEntityService = personEntityService;
        this.customerService = customerService;
    }

    public IActionResult Index()
    {
        return View();
    }
}
