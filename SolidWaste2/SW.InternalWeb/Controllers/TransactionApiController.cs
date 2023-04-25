using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PE.BL.Services;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Extensions;
using SW.InternalWeb.Models.Transaction;

namespace SW.InternalWeb.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TransactionApiController : ControllerBase
{
    private readonly IContainerService _containerService;
    private readonly IContainerRateService _containerRateService;
    private readonly ICustomerService _customerService;
    private readonly IPaymentPlanService _paymentPlanService;
    private readonly IServiceAddressService _serviceAddressService;
    private readonly ITransactionService _transactionService;
    private readonly ITransactionCodeService _transactionCodeService;
    private readonly ITransactionCodeRuleService _transactionCodeRuleService;
    private readonly ITransactionHoldingService _transactionHoldingService;
    private readonly IAddressService _addressService;
    private readonly IPersonEntityService _personEntityService;

    public TransactionApiController(
        IContainerService containerService,
        IContainerRateService containerRateService,
        ICustomerService customerService,
        IPaymentPlanService paymentPlanService,
        IServiceAddressService serviceAddressService,
        ITransactionService transactionService,
        ITransactionCodeService transactionCodeService,
        ITransactionCodeRuleService transactionCodeRuleService,
        ITransactionHoldingService transactionHoldingService,
        IAddressService addressService,
        IPersonEntityService personEntityService)
    {
        _containerService = containerService;
        _containerRateService = containerRateService;
        _customerService = customerService;
        _paymentPlanService = paymentPlanService;
        _serviceAddressService = serviceAddressService;
        _transactionService = transactionService;
        _transactionCodeService = transactionCodeService;
        _transactionCodeRuleService = transactionCodeRuleService;
        _transactionHoldingService = transactionHoldingService;
        _addressService = addressService;
        _personEntityService = personEntityService;
    }

    [HttpGet]
    public async Task<IActionResult> CheckId(int customerId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure CustomerID is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("Error: CustomerID invalid");

            var person = await _personEntityService.GetById(customer.Pe);

            var jsonResult = new { customerName = person.FullName, transactionCodeSelectList = await GenerateTransactionCodeSelectListGroupS() };
            return Ok(jsonResult);
        }
        catch(Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckCode(int customerId, int transactionCodeId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure CustomerID is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("Error: CustomerID invalid");

            // Make sure TransactionCodeID is selected
            if (transactionCodeId == 0)
                throw new ArgumentException("Error: TransactionCodeID not selected");

            // Make sure TransactionCodeID is valid
            var code = await _transactionCodeService.GetById(transactionCodeId);
            if (code == null)
                throw new ArgumentException("Error: TransactionCodeID invalid");

            var jsonResult = new { serviceAddressSelectList = await GenerateServiceAddressSelectList(customer.CustomerId, code.TransactionCodeId) };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckServiceAddress(int transactionCodeId, int serviceAddressId)
    {
        try
        {
            // Make sure TransactionCodeID is selected
            if (transactionCodeId == 0)
                throw new ArgumentException("Error: TransactionCodeID not selected");

            // Make sure TransactionCodeID is valid
            var code = await _transactionCodeService.GetById(transactionCodeId);
            if (code == null)
                throw new ArgumentException("Error: TransactionCodeID invalid");

            // Make sure ServiceAddressID is selected
            if (serviceAddressId == 0)
                throw new ArgumentException("Error: ServiceAddressID not selected");

            // Make sure ServiceAddressID is valid
            var address = await _serviceAddressService.GetById(serviceAddressId);
            if (address == null)
                throw new ArgumentException("Error: ServiceAddressID invalid");

            var rules = await _transactionCodeRuleService.GetByTransactionCodeId(code.TransactionCodeId);

            Formula formula = null;

            if (rules.Count == 1)
                formula = rules.First().Formula;

            // test if this is actually necessary? SOWA-92
            if (formula != null && formula.Parameters != null)
            {
                foreach (var p in formula.Parameters)
                {
                    p.Formula = null;
                }
            }

            var jsonResult = new { code = code.TransactionCodeId, formula, containerSelectList = await GenerateContainerSelectList(address.Id, rules) };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckContainer(int transactionCodeId, int containerId)
    {
        try
        {
            // Make sure TransactionCodeID is selected
            if (transactionCodeId == 0)
                throw new ArgumentException("Error: TransactionCodeID not selected");

            // Make sure TransactionCodeID is valid
            var code = await _transactionCodeService.GetById(transactionCodeId);
            if (code == null)
                throw new ArgumentException("Error: TransactionCodeID invalid");

            // Make sure ContainerID is selected
            if (containerId == 0)
                throw new ArgumentException("Error: ContainerID not selected");

            var rules = new List<TransactionCodeRule>();

            decimal transactionAmount = 0;

            if (transactionCodeId == 48 && containerId == -1)
            {
                rules = (await _transactionCodeRuleService.GetByTransactionCodeId(transactionCodeId)).ToList();
            }
            else
            {
                // Make sure ContainerID is valid
                var container = await _containerService.GetById(containerId);
                if (container == null)
                    throw new ArgumentException("Error: ContainerID invalid");

                // Make sure there is a matching ContainerRate for the given Container
                var containerRate = (await _containerRateService
                    .GetByCodeDaysSizeEffDate(
                        container.ContainerCodeId, 
                        container.ContainerSubtypeId, 
                        container.NumDaysService, 
                        container.BillingSize,
                        container.EffectiveDate > DateTime.Now ? container.EffectiveDate : DateTime.Now))
                    .FirstOrDefault();
                if (containerRate == null)
                    throw new ArgumentException("Error: Container has no matching ContainerRate");

                rules = (await _transactionCodeRuleService.GetByContainerAndTransactionCode(container, code.TransactionCodeId)).ToList();
                transactionAmount = code.Code == "EP" ? containerRate.ExtraPickup : containerRate.PullCharge;
            }

            // Make sure there is exactly one TransactionCodeRule for the given container
            if (rules.Count == 0)
                throw new ArgumentException("Error: No formula found");
            if (transactionCodeId != 48 && containerId != -1 && rules.Count > 1)
                throw new ArgumentException("Error: More than one formula found");

            var formula = rules.First().Formula;

            // test if this is actually necessary? SOWA-92
            if (formula != null && formula.Parameters != null)
            {
                foreach (var p in formula.Parameters)
                {
                    p.Formula = null;
                }
            }

            if (code.Code == "CR")
                transactionAmount = 75.00m;

            var jsonResult = new { formula, transAmt = transactionAmount };
            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckIDPayment(int customerId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure CustomerID is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("Error: CustomerID invalid");

            var person = await _personEntityService.GetById(customer.Pe);

            var paymentPlan = (await _paymentPlanService.GetByCustomer(customer.CustomerId, true)).SingleOrDefault(m => m.Status == "Active");

            // test if this is actually necessary? SOWA-92
            if (paymentPlan != null)
            {
                foreach (var p in paymentPlan.Details)
                {
                    p.PaymentPlan = null;
                }
            }

            var jsonResult = new
            {
                customerName = person.FullName,
                currentBalance = await _transactionService.GetCurrentBalance(customer.CustomerId),
                paymentPlan = paymentPlan,
                counselorsBalance = await _transactionService.GetCounselorsBalance(customer.CustomerId),
                collectionsBalance = await _transactionService.GetCollectionsBalance(customer.CustomerId),
                transactionHoldingSelectList = await GenerateTransactionHoldingSelectList(customer.CustomerId),
                transactionCodeSelectList = await GenerateTransactionCodeSelectListGroupP(customer.PaymentPlan)
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
    public async Task<IActionResult> CheckTransactionHoldingPayment(int customerId, int transactionHoldingId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure CustomerID is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("Error: CustomerID invalid");

            // IF : TransactionHoldingID selected
            if (transactionHoldingId != 0)
            {
                // Make sure TransactionHoldingID is valid
                var transactionHolding = await _transactionHoldingService.GetById(transactionHoldingId);
                if (transactionHolding == null)
                    throw new ArgumentException("Error: TransactionHoldingID invalid");

                // Make sure TransactionHolding belongs to the given customer
                if (transactionHolding.CustomerId != customer.CustomerId)
                    throw new ArgumentException("Error: HoldingTransactionID does not belong to given CustomerID");

                var transactionHoldingDetail = new TransactionHoldingDetailJson
                {
                    AddDateTime = transactionHolding.AddDateTime.ToShortDateString(),
                    Sender = transactionHolding.Sender,
                    Status = transactionHolding.Status,
                    TransactionAmt = transactionHolding.TransactionAmt.ToString(),
                    TransactionDescription = transactionHolding.TransactionCode.Description
                };

                var jsonResult = new { transactionHolding = transactionHoldingDetail };
                return Ok(jsonResult);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            var jsonResult = new { message = ex.Message };
            return Ok(jsonResult);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckIDAdjustment(int customerId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure CustomerID is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("Error: CustomerID invalid");

            var person = await _personEntityService.GetById(customer.Pe);

            var jsonResult = new
            {
                customerName = person.FullName,
                currentBalance = await _transactionService.GetCurrentBalance(customer.CustomerId),
                counselorsBalance = await _transactionService.GetCounselorsBalance(customer.CustomerId),
                collectionsBalance = await _transactionService.GetCollectionsBalance(customer.CustomerId),
                transactionCodeSelectList = await GenerateTransactionCodeSelectListGroupA()
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
    public async Task<IActionResult> CheckTransactionCodeAdjustment(int customerId, int transactionCodeId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure TransactionCodeID is selected
            if (transactionCodeId == 0)
                throw new ArgumentException("Error: TransactionCodeID not selected");

            // Make sure TransactionCodeID is valid
            var transactionCode = await _transactionCodeService.GetById(transactionCodeId);
            if (transactionCode == null)
                throw new ArgumentException("Error: TransactionCodeID invalid");

            var isAssociatedTransactionIdRequired = transactionCode.Code == "LFR";
            var associatedTransactionSelectList = await GenerateAssociatedTransactionSelectList(customerId);

            var jsonResult = new
            {
                // SOWA-92  need to replace with new role info once that is setup
                //security = ConfigurationManager.AppSettings["Access.Admin"],
                security = @"SNCO\ITData",
                isAssociatedTransactionIdRequired,
                associatedTransactionSelectList
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
    public async Task<IActionResult> BatchAjaxCustomerID(int customerId)
    {
        try
        {
            // Make sure CustomerID is selected
            if (customerId == 0)
                throw new ArgumentException("Error: CustomerID not selected");

            // Make sure CustomerID is valid
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new ArgumentException("Error: CustomerID invalid");

            var person = await _personEntityService.GetById(customer.Pe);

            var paymentPlan = (await _paymentPlanService.GetByCustomer(customer.CustomerId, true)).SingleOrDefault(m => m.Status == "Active");

            // test if this is actually necessary? SOWA-92
            if (paymentPlan != null)
            {
                foreach (var p in paymentPlan.Details)
                {
                    p.PaymentPlan = null;
                }
            }

            var jsonResult = new
            {
                customerName = person.FullName,
                paymentPlan,
                currentBalance = await _transactionService.GetCurrentBalance(customer.CustomerId),
                counselorsBalance = await _transactionService.GetCounselorsBalance(customer.CustomerId),
                collectionsBalance = await _transactionService.GetCollectionsBalance(customer.CustomerId),
                transactionCodeSelectList = await GenerateTransactionCodeSelectListGroupP(customer.PaymentPlan)
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
    public async Task<IActionResult> GetTransactionCodeSign(int transactionCodeId)
    {
        try
        {
            // Make sure TransactionCodeID is valid
            var transactionCode = await _transactionCodeService.GetById(transactionCodeId);
            if (transactionCode == null)
                throw new ArgumentException("Error: TransactionCodeID invalid");

            var jsonResult = new
            {
                sign = transactionCode.TransactionSign
            };

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
    private async Task<IEnumerable<SelectListItem>> GenerateTransactionHoldingSelectList(int customerId)
    {
        var transactionHoldings = await _transactionHoldingService.GetAllAwaitingPaymentByCustomerId(customerId, false);

        return transactionHoldings.Select(th => new SelectListItem
        {
            Text = string.Format("{0} : {1} ${2} {3}", th.TransactionHoldingId, th.TransactionCode.Description, th.TransactionAmt, th.AddDateTime.ToString("MM/dd/yyyy")),
            Value = th.TransactionHoldingId.ToString()
        });
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateTransactionCodeSelectListGroupS()
    {
        var transactionCodes = (await _transactionCodeService.GetAll()).Where(c => c.Group == "S");

        return transactionCodes.Select(tc => new SelectListItem
        {
            Text = tc.Code.PadRight(5, ' ') + ": " + tc.Description,
            Value = tc.TransactionCodeId.ToString()
        });
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateTransactionCodeSelectListGroupP(bool paymentPlan)
    {
        var transactionCodes = (await _transactionCodeService.GetAll()).Where(tc => tc.Group == "P");

        return transactionCodes
            .Where(tc => (paymentPlan && tc.Code == "PP") || (!paymentPlan && tc.Code != "PP"))
            .Select(tc => new SelectListItem
        {
            Text = tc.Code.PadRight(5, ' ') + ": " + tc.Description,
            Value = tc.TransactionCodeId.ToString()
        });
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateTransactionCodeSelectListGroupA()
    {
        var transactionCodes = (await _transactionCodeService.GetAll()).Where(c => c.Group == "A");

        return transactionCodes.Select(tc => new SelectListItem
        {
            Text = tc.Code.PadRight(5, ' ') + ": " + tc.Description,
            Value = tc.TransactionCodeId.ToString()
        });
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateServiceAddressSelectList(int customerId, int transactionCodeId)
    {
        var list = new List<SelectListItem>();

        var transactionCodeRules = await _transactionCodeRuleService.GetByTransactionCodeId(transactionCodeId);
        if (!transactionCodeRules.Any())
            return list;

        var serviceAddresses = await _serviceAddressService.GetByCustomer(customerId);
        if (!serviceAddresses.Any())
            return list;

        var anyRules = transactionCodeRules.Any(tc => tc.ContainerCodeId == null);

        foreach (var sa in serviceAddresses)
        {
            var containers = sa.Containers.Where(c => !c.DeleteFlag);

            var anyContainers = containers.Any(c => transactionCodeRules
                .Any(t => t.ContainerCodeId == c.ContainerCodeId
                    && t.ContainerSubtypeId == c.ContainerSubtypeId
                    && t.ContainerNumDaysService == c.NumDaysService
                    && t.ContainerBillingSize == c.BillingSize)
                );

            if (anyRules || anyContainers)
            {
                var address = await _addressService.GetById(sa.PeaddressId);

                var text = string.Format("LOC {0} : {1}", sa.LocationNumber ?? "", address.ToFullString());
                if (sa.CancelDate.HasValue && sa.CancelDate < DateTime.Now)
                    text += " (CANCELED)";

                list.Add(new SelectListItem { Value = sa.Id.ToString(), Text = text });
            }
        }

        return list;
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateAssociatedTransactionSelectList(int customerId)
    {
        var associatedTransactions = await _transactionService.GetAllUnpaidLateFeesByCustomerId(customerId);

        return associatedTransactions.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = string.Format("{0} | {1} | {2}", t.AddDateTime.ToString("yyyy/MM/dd HH:mm:ss"), t.TransactionCode.Description, t.TransactionAmt)
        });
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateContainerSelectList(int serviceAddressId, ICollection<TransactionCodeRule> transactionCodeRules)
    {
        var list = new List<SelectListItem>();

        if (!transactionCodeRules.Any())
            return list;

        var containers = await _containerService.GetByServiceAddress(serviceAddressId);
        if (!containers.Any())
            return list;

        var validContainers = containers.Where(c => transactionCodeRules
            .Any(t => t.ContainerCodeId == c.ContainerCodeId
                && t.ContainerSubtypeId == c.ContainerSubtypeId
                && t.ContainerNumDaysService == c.NumDaysService
                && t.ContainerBillingSize == c.BillingSize)
            );
        
        foreach (var c in validContainers)
        {
            var rate = (await _containerRateService.GetByCodeDaysSizeEffDate(c.ContainerCodeId, c.ContainerSubtypeId, c.NumDaysService, c.BillingSize, DateTime.Now)).FirstOrDefault();
            if (rate == null || rate.PullCharge == 0m)
                continue;

            string text = string.Format("TYPE {0} - {1}, SIZE {2}, ROUTE {3}, DAYS {4}", c.ContainerCode.Type, c.ContainerSubtype.Description, c.BillingSize.ToString(), c.RouteNumber, c.NumDaysService);
            if (c.CancelDate.HasValue && c.CancelDate < DateTime.Now)
                text += " (CANCELED)";
            list.Add(new SelectListItem { Value = c.Id.ToString(), Text = text });
        }

        return list;
    }

    #endregion
}
