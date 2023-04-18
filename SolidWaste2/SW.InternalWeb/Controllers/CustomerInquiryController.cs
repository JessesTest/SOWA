using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.InternalWeb.Models.CustomerInquiry;

namespace SW.InternalWeb.Controllers;

public class CustomerInquiryController : Controller
{
    private readonly ICustomerInquiryService customerInquiryService;

    public CustomerInquiryController(ICustomerInquiryService customerInquiryService)
    {
        this.customerInquiryService = customerInquiryService;
    }

    public IActionResult Index()
    {
        return View(new CustomerInquiryParametersViewModel());
    }

    public async Task<IActionResult> Search(CustomerInquiryParametersViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model).WithWarning("There are errors on the form", "");

        switch (model.Command)
        {
            case "search":
                model.Results = await customerInquiryService.Search(
                    model.CustomerNumber,
                    string.IsNullOrWhiteSpace(model.CustomerName) ? null : model.CustomerName.Trim(),
                    model.CustomerAddress,
                    //model.Route,
                    model.LocationName,
                    model.PIN,
                    model.LocationAddress,
                    model.Include);
                break;
            case "clear":
                model = new CustomerInquiryParametersViewModel
                {
                    Include = true
                };
                break;
        }

        if (model.CustomerNumber.HasValue && model.Results.Count() == 1)
        {
            model.CustomerNumber = model.Results.First().Customer.CustomerId;
        }

        return View("Index", model);
    }
}
