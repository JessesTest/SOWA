using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PE.BL.Services;
using SW.BLL.Services;
using SW.InternalWeb.Extensions;

namespace SW.InternalWeb.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class WorkOrderApiController : ControllerBase
{
    private readonly IContainerService _containerService;
    private readonly IContainerCodeService _containerCodeService;
    private readonly IContainerRateService _containerRateService;
    private readonly ICustomerService _customerService;
    private readonly IRouteTypeService _routeTypeService;
    private readonly IServiceAddressService _serviceAddressService;
    private readonly IAddressService _addressService;
    private readonly IPersonEntityService _personEntityService;

    public WorkOrderApiController(
        IContainerService containerService,
        IContainerCodeService containerCodeService,
        IContainerRateService containerRateService,
        ICustomerService customerService,
        IRouteTypeService routeTypeService,
        IServiceAddressService serviceAddressService,
        IAddressService addressService,
        IPersonEntityService personEntityService)
    {
        _containerService = containerService;
        _containerCodeService = containerCodeService;
        _containerRateService = containerRateService;
        _customerService = customerService;
        _routeTypeService = routeTypeService;
        _serviceAddressService = serviceAddressService;
        _addressService = addressService;
        _personEntityService = personEntityService;
    }

    [HttpGet]
    public async Task<IActionResult> ServiceAddressIdLoad(int customerId)
     {
        try
        {
            // Make sure CustomerId is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("[CustomerId] invalid");

            var jsonResult = new { serviceAddressSelectList = await GenerateServiceAddressSelectList(customer.CustomerId) };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContainerIdLoad(int serviceAddressId)
    {
        try
        {
            // Make sure ServiceAddressId is valid
            var serviceAddress = await _serviceAddressService.GetById(serviceAddressId);
            if (serviceAddress == null)
                throw new ArgumentException("[ServiceAddressId] invalid");

            var jsonResult = new { containerSelectList = await GenerateContainerSelectList(serviceAddress.Id) };

            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContainerCodeLoad()
    {
        try
        {
            var jsonResult = new { containerCodeSelectList = await GenerateContainerCodeSelectListNotId() };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContainerRouteLoad()
    {
        try
        {
            var jsonResult = new { containerRouteSelectList = await GenerateContainerRouteSelectList() };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContainerSizeLoad(string containerCode, int dayCount)
    {
        try
        {
            var jsonResult = new { containerSizeSelectList = await GenerateContainerSizeSelectList(containerCode, dayCount) };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CustomerIdChange(int customerId)
    {
        try
        {
            // Make sure CustomerId is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("[CustomerId] invalid");

            // Get matching PersonEntity and Billing Address
            var personEntity = await _personEntityService.GetById(customer.Pe);
            var address = (await _addressService.GetByPerson(personEntity.Id, false)).SingleOrDefault(m => m.Type == 9);

            var jsonResult = new
            {
                customerId = customer.CustomerId,
                serviceAddressSelectList = await GenerateServiceAddressSelectList(customer.CustomerId),
                customerType = customer.CustomerType,
                customerName = personEntity.FullName,
                customerAddress = address.ToFullString()
            };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ServiceAddressIdChange(int serviceAddressId)
    {
        try
        {
            // Make sure ServiceAddressId is valid
            var serviceAddress = await _serviceAddressService.GetById(serviceAddressId);
            if (serviceAddress == null)
                throw new ArgumentException("[ServiceAddressId] invalid");

            var jsonResult = new { containerSelectList = await GenerateContainerSelectList(serviceAddressId) };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContainerIdChange(int containerId)
    {
        try
        {
            // Make sure ContainerId is valid
            var container = await _containerService.GetById(containerId);
            if (container == null)
                throw new ArgumentException("[ContainerId] invalid");

            var jsonResult = new
            {
                containerRoute = container.RouteNumber,
                pickupMon = container.MonService,
                pickupTue = container.TueService,
                pickupWed = container.WedService,
                pickupThu = container.ThuService,
                pickupFri = container.FriService,
                pickupSat = container.SatService,
                containerType = container.ContainerCode.Type,
                containerSize = container.BillingSize.ToString("N1")
            };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public IActionResult ContainerCodeChange()
    {
        try
        {
            var jsonResult = new { };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    #region Helpers

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateContainerCodeSelectListNotId()
    {
        var containerCodes = await _containerCodeService.GetAll();

        return containerCodes.Select(c => new SelectListItem 
        { 
            Value = c.Type, 
            Text = c.Type + " - " + c.Description 
        }).ToList();
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateServiceAddressSelectList(int customerId)
    {
        var serviceAddresses = await _serviceAddressService.GetByCustomer(customerId);
        var list = new List<SelectListItem>();

        foreach (var sa in serviceAddresses)
        {
            var address = await _addressService.GetById(sa.PeaddressId);

            var text = string.Format("LOC {0} : {1}", sa.LocationNumber ?? " ", address.ToFullString());
            if (sa.CancelDate.HasValue && sa.CancelDate < DateTime.Now)
                text += " (CANCELED)";

            list.Add(new SelectListItem { Value = sa.Id.ToString(), Text = text });
        }

        return list;
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateContainerSelectList(int serviceAddressID)
    {
        var containers = await _containerService.GetByServiceAddress(serviceAddressID);
        var list = new List<SelectListItem>();

        foreach (var c in containers)
        {
            var numDays = 0;
            numDays += c.MonService ? 1 : 0;
            numDays += c.TueService ? 1 : 0;
            numDays += c.WedService ? 1 : 0;
            numDays += c.ThuService ? 1 : 0;
            numDays += c.FriService ? 1 : 0;
            numDays += c.SatService ? 1 : 0;

            var text = string.Format("TYPE {0} - {1}, SIZE {2}, ROUTE {3}, DAYS {4}", c.ContainerCode.Type, c.ContainerSubtype.Description, c.BillingSize.ToString(), c.RouteNumber, numDays);
            if (c.CancelDate.HasValue && c.CancelDate < DateTime.Now)
                text += " (CANCELED)";

            list.Add(new SelectListItem { Value = c.Id.ToString(), Text = text });
        }

        return list;
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateContainerRouteSelectList()
    {
        var routeTypes = (await _routeTypeService.GetAll()).OrderBy(r => r.RouteNumber);

        return routeTypes.Select(r => new SelectListItem { Value = r.RouteNumber.ToString(), Text = r.RouteNumber.ToString() }).ToList();
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateContainerSizeSelectList(string type, int dayCount)
    {
        var containerCode = await _containerCodeService.GetByType(type);
        if (containerCode == null)
            return new List<SelectListItem>();

        var containerRates = await _containerRateService.GetContainerRateByCodeDays(containerCode.ContainerCodeId, dayCount);

        return containerRates
            .DistinctBy(c => c.BillingSize)
            .Select(c => new SelectListItem { Value = c.BillingSize.ToString("N1"), Text = c.BillingSize.ToString("N1") });
    }

    #endregion
}
