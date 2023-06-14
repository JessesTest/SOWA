using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notify.BL.Services;
using Notify.DM;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.Transaction;

namespace SW.InternalWeb.Controllers;

public class TransactionController : Controller
{
    private readonly IContainerService _containerService;
    private readonly IContainerRateService _containerRateService;
    private readonly ICustomerService _customerService;
    private readonly IServiceAddressService _serviceAddressService;
    private readonly ITransactionService _transactionService;
    private readonly ITransactionCodeService _transactionCodeService;
    private readonly ITransactionHoldingService _transactionHoldingService;
    private readonly INotifyService _notifyService;

    public TransactionController(
        IContainerService containerService,
        IContainerRateService containerRateService,
        ICustomerService customerService,
        IServiceAddressService serviceAddressService,
        ITransactionService transactionService,
        ITransactionCodeService transactionCodeService,
        ITransactionHoldingService transactionHoldingService,
        INotifyService notifyService)
    {
        _containerService = containerService;
        _containerRateService = containerRateService;
        _customerService = customerService;
        _serviceAddressService = serviceAddressService;
        _transactionService = transactionService;
        _transactionCodeService = transactionCodeService;
        _transactionHoldingService = transactionHoldingService;
        _notifyService = notifyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string customerID, string transactionCodeID, string serviceAddressID, string containerID)
    {
        var vm = new TransactionViewModel();
        int startLocks = 0;
        int validLocks = 0;

        if (!string.IsNullOrWhiteSpace(customerID))
        {
            startLocks++;
            if (!string.IsNullOrWhiteSpace(transactionCodeID))
            {
                startLocks++;
                if (!string.IsNullOrWhiteSpace(serviceAddressID))
                {
                    startLocks++;
                    if (!string.IsNullOrWhiteSpace(containerID))
                        startLocks++;
                }
            }
        }

        try
        {
            // IF : CustomerID provided as parameter
            if (!string.IsNullOrWhiteSpace(customerID))
            {
                var customer = await GetValidCustomer(customerID);

                // Map CustomerID to view model
                vm.CustomerID = customer.CustomerId.ToString();
                validLocks++;

                // IF : TransactionCodeID provided as parameter
                if (!string.IsNullOrWhiteSpace(transactionCodeID))
                {
                    var transactionCode = await GetValidTransactionCode(transactionCodeID, 0);

                    // Map TransactionCodeID to view model
                    vm.TransactionCodeID = transactionCode.TransactionCodeId;
                    validLocks++;

                    // IF : ServiceAddressID provided as parameter
                    if (!string.IsNullOrWhiteSpace(serviceAddressID))
                    {
                        var serviceAddress = await GetValidServiceAddress(serviceAddressID, 0, customer.CustomerId);

                        // Map ServiceAddressID to view model
                        vm.ServiceAddressID = serviceAddress.Id;
                        validLocks++;

                        // IF : ContainerID provided as parameter
                        if (!string.IsNullOrWhiteSpace(containerID))
                        {
                            var container = await GetValidContainer(containerID, 0, serviceAddress.Id);

                            // Map ContainerID to view model
                            vm.ContainerID = container.Id;
                            validLocks++;
                        }
                    }
                }
            }

            vm.Locks = validLocks;

            return View(vm);
        }
        catch (Exception ex)
        {
            return (validLocks > startLocks ? startLocks : validLocks) switch
            {
                1 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID }).WithDanger(ex.Message, ""),
                2 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID, transactionCodeID = vm.TransactionCodeID }).WithDanger(ex.Message, ""),
                3 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID, transactionCodeID = vm.TransactionCodeID, serviceAddressID = vm.ServiceAddressID }).WithDanger(ex.Message, ""),
                4 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID, transactionCodeID = vm.TransactionCodeID, serviceAddressID = vm.ServiceAddressID, containerID = vm.ContainerID }).WithDanger(ex.Message, ""),
                _ => RedirectToAction(nameof(Index)).WithDanger(ex.Message, "")
            };
        }
    }

    [HttpPost]
    public async Task<IActionResult> Index(TransactionViewModel vm)
    {
        int validLocks = 0;

        try
        {
            var transaction = new Transaction();

            // Make sure CustomerID is selected
            if (string.IsNullOrWhiteSpace(vm.CustomerID))
                throw new ArgumentException("CustomerID not selected");

            var customer = await GetValidCustomer(vm.CustomerID);

            // CustomerID passed validation
            vm.CustomerID = customer.CustomerId.ToString();
            validLocks++;

            // Make sure TransactionCodeID is selected
            if (vm.TransactionCodeID == 0)
                throw new ArgumentException("TransactionCodeID not selected");

            var transactionCode = await GetValidTransactionCode(null, vm.TransactionCodeID);

            // TransactionCodeID passed validation
            vm.TransactionCodeID = transactionCode.TransactionCodeId;
            validLocks++;

            // Make sure ServiceAddressID is selected
            if (vm.ServiceAddressID == 0)
                throw new ArgumentException("ServiceAddressID not selected");

            var serviceAddress = await GetValidServiceAddress(null, vm.ServiceAddressID, customer.CustomerId);

            // ServiceAddressID passed validation
            vm.ServiceAddressID = serviceAddress.Id;
            validLocks++;

            if (transactionCode.Code != "BK" && transactionCode.Code != "CR")
            {
                // Make sure ContainerID is selected
                if (vm.ContainerID == 0)
                    throw new ArgumentException("ContainerID not selected");

                var container = await GetValidContainer(null, vm.ContainerID, serviceAddress.Id);

                // ContainerID passed validation
                vm.ContainerID = container.Id;
                validLocks++;

                var containerRate = (await _containerRateService
                    .GetByCodeDaysSize(
                        container.ContainerSubtypeId,
                        container.NumDaysService,
                        container.BillingSize,
                        container.EffectiveDate > DateTime.Today ? container.EffectiveDate : DateTime.Today))
                    .FirstOrDefault();
                if (containerRate != null)
                    transaction.ObjectCode = containerRate.ObjectCode;
            }
            else
            {
                // Make sure ContainerID is -1
                if (vm.ContainerID != -1)
                    throw new ArgumentException("ContainerID value unexpected");

                transaction.ObjectCode = 43201;
                validLocks++;
            }

            // Make sure TransactionAmount has a decimal value
            if (!decimal.TryParse(vm.TransactionAmount, out decimal transactionAmt))
                throw new ArgumentException("TransactionAmount not a decimal value");

            // Make sure WorkOrder does not exceed 16 characters
            if (!string.IsNullOrWhiteSpace(vm.WorkOrder) && vm.WorkOrder.Length > 16)
                throw new ArgumentException("WorkOrder exceeds 16 characters");

            // Map Transaction data
            transaction.CustomerId = customer.CustomerId;
            transaction.TransactionCodeId = transactionCode.TransactionCodeId;
            transaction.TransactionAmt = transactionAmt;
            transaction.WorkOrder = vm.WorkOrder?.Trim();
            transaction.Comment = vm.Comment == null ? string.Empty : vm.Comment.Replace("{DateTime.Now}", DateTime.Now.ToString("MM/dd/yyyy"));
            transaction.ServiceAddressId = vm.ServiceAddressID;
            transaction.ContainerId = vm.ContainerID == -1 ? null : vm.ContainerID;
            transaction.AddToi = User.GetNameOrEmail();

            // IF : TransactionCode is a type that needs holding
            if (transactionCode.Hold)
            {
                // Map TransactionHolding data
                var transactionHolding = new TransactionHolding
                {
                    CustomerId = transaction.CustomerId,
                    TransactionAmt = transaction.TransactionAmt,
                    TransactionCodeId = transaction.TransactionCodeId,
                    WorkOrder = transaction.WorkOrder,
                    Comment = transaction.Comment,
                    ServiceAddressId = transaction.ServiceAddressId,
                    ContainerId = transaction.ContainerId,
                    ObjectCode = transaction.ObjectCode,
                    AssociatedTransactionId = transaction.AssociatedTransactionId,
                    Status = "Awaiting Payment",

                    AddToi = User.GetNameOrEmail(),
                    Sender = User.GetEmail()
                };

                // Submit TransactionHolding and return to Pending Work view
                await _transactionHoldingService.Add(transactionHolding);
                return RedirectToAction("Personal", "PendingWork");
            }

            // Submit Transaction and return to Customer Transaction History view
            var result = await _transactionService.AddTransactionAutoCalc(transaction);
            if (result != null)
                throw new ArgumentException(result);

            return RedirectToAction("Index", "CustomerTransactionHistory", new { customerID = customer.CustomerId });
        }
        catch (Exception ex)
        {
            return (validLocks > vm.Locks ? vm.Locks : validLocks) switch
            {
                1 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID }).WithDanger(ex.Message, ""),
                2 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID, transactionCodeID = vm.TransactionCodeID }).WithDanger(ex.Message, ""),
                3 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID, transactionCodeID = vm.TransactionCodeID, serviceAddressID = vm.ServiceAddressID }).WithDanger(ex.Message, ""),
                4 => RedirectToAction(nameof(Index), new { customerID = vm.CustomerID, transactionCodeID = vm.TransactionCodeID, serviceAddressID = vm.ServiceAddressID, containerID = vm.ContainerID }).WithDanger(ex.Message, ""),
                _ => RedirectToAction(nameof(Index)).WithDanger(ex.Message, "")
            };
        }
    }

    [HttpGet]
    public async Task<IActionResult> Payment(string customerID, string transactionHoldingID)
    {
        var vm = new TransactionPaymentViewModel();
        int startLocks = 0;
        int validLocks = 0;

        if (!string.IsNullOrWhiteSpace(customerID))
        {
            startLocks++;
            if (!string.IsNullOrWhiteSpace(transactionHoldingID))
            {
                startLocks++;
            }
        }

        try
        {
            // IF : CustomerID provided as parameter
            if (!string.IsNullOrWhiteSpace(customerID))
            {
                var customer = await GetValidCustomer(customerID);

                // Map CustomerID to view model
                vm.CustomerID = customer.CustomerId.ToString();
                validLocks++;

                // IF : TransactionHoldingID provided as parameter
                if (!string.IsNullOrWhiteSpace(transactionHoldingID))
                {
                    var transactionHolding = await GetValidTransactionHolding(transactionHoldingID, 0, customer.CustomerId);

                    // Map TransactionHoldingID to view model
                    vm.TransactionHoldingID = transactionHolding.TransactionHoldingId;
                    validLocks++;
                }
            }

            vm.Locks = validLocks;

            return View(vm);
        }
        catch (Exception ex)
        {
            return (validLocks > startLocks ? startLocks : validLocks) switch
            {
                1 => RedirectToAction(nameof(Payment), new { customerID = vm.CustomerID }).WithDanger(ex.Message, ""),
                2 => RedirectToAction(nameof(Payment), new { customerID = vm.CustomerID, transactionHoldingID = vm.TransactionHoldingID }).WithDanger(ex.Message, ""),
                _ => RedirectToAction(nameof(Payment)).WithDanger(ex.Message, "")
            };
        }
    }

    [HttpPost]
    public async Task<IActionResult> Payment(TransactionPaymentViewModel vm)
    {
        int validLocks = 0;
        string result;

        try
        {
            var transaction = new Transaction();

            // Make sure CustomerID is selected
            if (string.IsNullOrWhiteSpace(vm.CustomerID))
                throw new ArgumentException("CustomerID not selected");

            var customer = await GetValidCustomer(vm.CustomerID);

            // Customer passed validation
            vm.CustomerID = customer.CustomerId.ToString();
            validLocks++;

            // Make sure TransactionCodeID is selected
            if (vm.TransactionCodeID == 0)
                throw new ArgumentException("TransactionCodeID not selected");

            var transactionCode = await GetValidTransactionCode(null, vm.TransactionCodeID);

            // TransactionCodeID passed validation
            vm.TransactionCodeID = transactionCode.TransactionCodeId;
            validLocks++;

            // Make sure TransactionAmount has a decimal value
            if (!decimal.TryParse(vm.TransactionAmount, out decimal transactionAmt))
                throw new ArgumentException("TransactionAmount not a decimal value");

            // IF : CheckNumber is selected, Make sure the check number has an Int64 value
            long checkNumber = 0;
            if (!string.IsNullOrWhiteSpace(vm.CheckNumber) && !long.TryParse(vm.CheckNumber, out checkNumber))
                throw new ArgumentException("CheckNumber not an integer value");

            // IF : A TransactionHoldingID is selected
            if (vm.TransactionHoldingID.HasValue && vm.TransactionHoldingID.Value != 0)
            {
                var transactionHolding = await GetValidTransactionHolding(null, vm.TransactionHoldingID.Value, customer.CustomerId);

                // TransactionHoldingID passed validation
                vm.TransactionHoldingID = transactionHolding.TransactionHoldingId;
                validLocks++;

                // Make sure TransactionAmount matches TransactionHolding
                if (transactionHolding.TransactionAmt != transactionAmt)
                    throw new ArgumentException("TransactionAmount does not match for TransactionHoldingID");

                // Map Transaction data
                transaction.TransactionHoldingId = transactionHolding.TransactionHoldingId;

                // Resolve TransactionHolding
                result = await _transactionHoldingService.Resolve(transactionHolding, User.GetEmail(), User.GetNameOrEmail());
                if (result != null)
                    throw new ArgumentException(result);
            }

            // Map Transaction data
            transaction.CustomerId = customer.CustomerId;
            transaction.TransactionCodeId = transactionCode.TransactionCodeId;
            transaction.TransactionAmt = transactionAmt;
            transaction.CheckNumber = checkNumber == 0 ? null : checkNumber;
            transaction.Comment = vm.Comment;
            transaction.AddToi = User.GetNameOrEmail();

            // Submit Transaction and return to Customer Transaction History view
            result = await _transactionService.AddTransactionAutoCalc(transaction);
            if (result != null)
                throw new ArgumentException(result);

            return RedirectToAction("Index", "CustomerTransactionHistory", new { customerID = customer.CustomerId });
        }
        catch (Exception ex)
        {
            return (validLocks > vm.Locks ? vm.Locks : validLocks) switch
            {
                1 => RedirectToAction(nameof(Payment), new { customerID = vm.CustomerID }).WithDanger(ex.Message, ""),
                2 => RedirectToAction(nameof(Payment), new { customerID = vm.CustomerID, transactionHoldingID = vm.TransactionHoldingID }).WithDanger(ex.Message, ""),
                _ => RedirectToAction(nameof(Payment)).WithDanger(ex.Message, ""),
            };
        }
    }

    [HttpGet]
    public async Task<IActionResult> Adjustment(string customerID)
    {
        var vm = new TransactionAdjustmentViewModel();
        int startLocks = 0;
        int validLocks = 0;

        if (!string.IsNullOrWhiteSpace(customerID))
        {
            startLocks++;
        }

        try
        {
            // IF : CustomerID provided as parameter
            if (!string.IsNullOrWhiteSpace(customerID))
            {
                var customer = await GetValidCustomer(customerID);

                // Map CustomerID to view model
                vm.CustomerID = customer.CustomerId.ToString();
                validLocks++;
            }

            vm.Locks = validLocks;

            return View(vm);
        }
        catch (Exception ex)
        {
            return (validLocks > startLocks ? startLocks : validLocks) switch
            {
                1 => RedirectToAction(nameof(Adjustment), new { customerID = vm.CustomerID }).WithDanger(ex.Message, ""),
                _ => RedirectToAction(nameof(Adjustment)).WithDanger(ex.Message, "")
            };
        }
    }

    [HttpPost]
    public async Task<IActionResult> Adjustment(TransactionAdjustmentViewModel vm)
    {
        int validLocks = 0;

        try
        {
            // Make sure CustomerID is selected
            if (string.IsNullOrWhiteSpace(vm.CustomerID))
                throw new ArgumentException("CustomerID not selected");

            var customer = await GetValidCustomer(vm.CustomerID);

            // CustomerID passed validation
            vm.CustomerID = customer.CustomerId.ToString();
            validLocks++;

            // Make sure TransactionCodeID is selected
            if (vm.TransactionCodeID == 0)
                throw new ArgumentException("TransactionCodeID not selected");

            var transactionCode = await GetValidTransactionCode(null, vm.TransactionCodeID);

            // Make sure TransactionAmount has a decimal value
            if (!decimal.TryParse(vm.TransactionAmount, out decimal transactionAmt))
                throw new ArgumentException("Transaction amount not a decimal value");

            // Map Transaction data
            var transaction = new Transaction
            {
                CustomerId = customer.CustomerId,
                TransactionCodeId = transactionCode.TransactionCodeId,
                TransactionAmt = transactionAmt,
                AssociatedTransactionId = vm.AssociatedTransactionId,
                Comment = vm.Comment,
                AddToi = User.GetNameOrEmail()
            };

            // IF : TransactionCode is a type that needs security
            if (!string.IsNullOrWhiteSpace(transactionCode.Security))
            {
                // Make sure EmailAddress selected belongs to a valid member from the required security group
                var email = User.GetEmail();
                if (string.IsNullOrWhiteSpace(vm.EmailAddress))
                    throw new ArgumentException("No request email given");
                if (email.ToUpper().Trim() == vm.EmailAddress.ToUpper().Trim())
                    throw new ArgumentException("Cannot submit request to self");

                // Map TransactionHolding data
                var transactionHolding = new TransactionHolding
                {
                    CustomerId = transaction.CustomerId,
                    TransactionAmt = transaction.TransactionAmt,
                    TransactionCodeId = transaction.TransactionCodeId,
                    WorkOrder = transaction.WorkOrder,
                    Comment = transaction.Comment,
                    ServiceAddressId = transaction.ServiceAddressId,
                    ContainerId = transaction.ContainerId,
                    ObjectCode = transaction.ObjectCode,
                    AssociatedTransactionId = transaction.AssociatedTransactionId,
                    Status = "Awaiting Verification",
                    Security = vm.EmailAddress,

                    AddToi = User.GetNameOrEmail(),
                    Sender = email
                };

                // Build Notification
                var notification = new Notification()
                {
                    Body = "Please verify the transaction request by visiting the URL.",
                    From = email,
                    Subject = "Transaction Verification",
                    System = "Solid Waste",
                    To = vm.EmailAddress,
                    Url = Url.Action("Single", "PendingWork", new { transactionHoldingID = transactionHolding.TransactionHoldingId })
                };
                await _notifyService.Add(notification);

                // Submit TransactionHolding
                await _transactionHoldingService.Add(transactionHolding);

                return RedirectToAction("Personal", "PendingWork");
            }

            // Submit Transaction and return to Customer Transaction History view
            var result = await _transactionService.AddTransactionAutoCalc(transaction);
            if (result != null)
                throw new ArgumentException(result);

            return RedirectToAction("Index", "CustomerTransactionHistory", new { customerID = customer.CustomerId });
        }
        catch (Exception ex)
        {
            return (validLocks > vm.Locks ? vm.Locks : validLocks) switch
            {
                1 => RedirectToAction(nameof(Adjustment), new { customerID = vm.CustomerID }).WithDanger(ex.Message, ""),
                _ => RedirectToAction(nameof(Adjustment)).WithDanger(ex.Message, "")
            };
        }
    }

    [HttpGet]
    public async Task<IActionResult> Batch(int? batchID, int? transactionHoldingID)
    {
        var vm = new BatchTransactionViewModel
        {
            ActiveBatches = await _transactionHoldingService.GetActiveBatches(),
            TransactionCodeSelectList = await GenerateTransactionCodeSelectListGroupP()
        };

        try
        {
            // If given a batchID validate and map to viewmodel
            if (batchID.HasValue)
            {
                var isValid = await _transactionHoldingService.IsBatchIdValid(batchID.Value);
                if (!isValid)
                    return View(vm);

                var batchTransactions = await _transactionHoldingService.GetBatchTransactionsById(batchID.Value);
                vm.Transactions = batchTransactions.Select(bt => new BatchTransactionViewModel.BatchTransactionElement
                {
                    CheckNumber = bt.CheckNumber?.ToString(),
                    Comment = bt.Comment,
                    CustomerID = bt.CustomerId.ToString(),
                    TransactionAmt = bt.TransactionAmt.ToString(),
                    TransactionCodeID = bt.TransactionCodeId?.ToString(),
                    TransactionHoldingID = bt.TransactionHoldingId.ToString(),
                    WorkOrder = bt.WorkOrder
                });
                vm.BatchID = batchID.Value;
            }

            // If given a transactionHoldingID validate and map to viewmodel
            if (transactionHoldingID.HasValue)
            {
                var transactionHolding = await _transactionHoldingService.GetById(transactionHoldingID.Value);
                if (transactionHolding == null || transactionHolding.BatchId != batchID.Value)
                    throw new ArgumentException("Invalid transaction");

                vm.CurrentTransaction = new BatchTransactionViewModel.BatchTransactionElement
                {
                    CheckNumber = transactionHolding.CheckNumber?.ToString(),
                    Comment = transactionHolding.Comment,
                    CustomerID = transactionHolding.CustomerId.ToString(),
                    TransactionAmt = transactionHolding.TransactionAmt.ToString(),
                    TransactionCodeID = transactionHolding.TransactionCodeId?.ToString(),
                    TransactionHoldingID = transactionHolding.TransactionHoldingId.ToString(),
                    WorkOrder = transactionHolding.WorkOrder
                };
            }

            return View(vm);
        }
        catch (Exception ex)
        {
            return View(vm).WithDanger(ex.Message, "");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Batch(BatchTransactionViewModel vm)
    {
        try
        {
            // Create new batch number if needed
            var isValid = await _transactionHoldingService.IsBatchIdValid(vm.BatchID);
            if (!isValid)
                vm.BatchID = await _transactionHoldingService.GetNextBatchId();

            if (!string.IsNullOrWhiteSpace(vm.CurrentTransaction.TransactionHoldingID))
            {
                // Make sure TransactionHoldingID is valid
                if (!int.TryParse(vm.CurrentTransaction.TransactionHoldingID, out int transactionHoldingID))
                    throw new ArgumentException("TransactionHoldingID invalid");

                var transactionHolding = await _transactionHoldingService.GetById(transactionHoldingID);
                if (transactionHolding == null)
                    throw new ArgumentException("Invalid transaction");

                if (!string.IsNullOrWhiteSpace(vm.CurrentTransaction.CheckNumber))
                    transactionHolding.CheckNumber = long.TryParse(vm.CurrentTransaction.CheckNumber, out long checkNumber) ? 
                        checkNumber : throw new ArgumentException("CheckNumber not an integer value");

                transactionHolding.CustomerId = int.TryParse(vm.CurrentTransaction.CustomerID, out int customerId) ? 
                    customerId : throw new ArgumentException("CustomerID not an integer value");

                transactionHolding.TransactionAmt = decimal.TryParse(vm.CurrentTransaction.TransactionAmt, out decimal transactionAmt) ? 
                    transactionAmt : throw new ArgumentException("TransactionAmount not a decimal value");

                transactionHolding.TransactionCodeId = int.TryParse(vm.CurrentTransaction.TransactionCodeID, out int transactionCodeId) ? 
                    transactionCodeId : throw new ArgumentException("TransactionCodeID not an integer value");

                transactionHolding.Comment = vm.CurrentTransaction.Comment;
                transactionHolding.WorkOrder = vm.CurrentTransaction.WorkOrder;

                transactionHolding.TransactionCode = null;
                transactionHolding.ChgToi = User.GetNameOrEmail();
                transactionHolding.Sender = User.GetEmail();

                await _transactionHoldingService.Update(transactionHolding);
            }
            else
            {
                var transactionHolding = new TransactionHolding 
                { 
                    Comment = vm.CurrentTransaction.Comment,
                    WorkOrder = vm.CurrentTransaction.WorkOrder,
                    BatchId = vm.BatchID,
                    Status = "Awaiting Submit",

                    AddToi = User.GetNameOrEmail(),
                    Sender = User.GetEmail()
                };

                if (!string.IsNullOrWhiteSpace(vm.CurrentTransaction.CheckNumber))
                    transactionHolding.CheckNumber = long.TryParse(vm.CurrentTransaction.CheckNumber, out long checkNumber) ? 
                        checkNumber : throw new ArgumentException("CheckNumber not an integer value");

                transactionHolding.CustomerId = int.TryParse(vm.CurrentTransaction.CustomerID, out int customerId) ?
                    customerId : throw new ArgumentException("CustomerID not an integer value");

                transactionHolding.TransactionAmt = decimal.TryParse(vm.CurrentTransaction.TransactionAmt, out decimal transactionAmt) ?
                    transactionAmt : throw new ArgumentException("TransactionAmt not a decimal value");

                transactionHolding.TransactionCodeId = int.TryParse(vm.CurrentTransaction.TransactionCodeID, out int transactionCodeId) ? 
                    transactionCodeId : throw new ArgumentException("TransactionCodeID not an integer value");

                await _transactionHoldingService.Add(transactionHolding);
            }

            return RedirectToAction(nameof(Batch), new { batchID = vm.BatchID });
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Batch), new { batchID = vm.BatchID }).WithDanger(ex.Message, "");
        }
    }

    [HttpGet]
    public async Task<IActionResult> BatchSubmit(int batchID)
    {
        try
        {
            // Validate BatchID
            var isValid = await _transactionHoldingService.IsBatchIdValid(batchID);
            if (!isValid)
                throw new ArgumentException("BatchID invalid");

            // Get batch
            var batchTransactions = await _transactionHoldingService.GetBatchTransactionsById(batchID);
            // Submit batch
            foreach(var item in batchTransactions)
            {
                var result = await _transactionHoldingService.Resolve(item, User.GetEmail(), User.GetNameOrEmail());
                if (result != null)
                    throw new ArgumentException(result);
            }

            return RedirectToAction(nameof(Batch)).WithSuccess("Batch submitted", "");
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Batch), new { batchID }).WithDanger(ex.Message, "");
        }
    }

    [HttpGet]
    public async Task<IActionResult> BatchDeleteTransaction(int transactionHoldingID, int batchID)
    {
        try
        {
            // Validate TransactionHolding
            var transactionHolding = await _transactionHoldingService.GetById(transactionHoldingID);
            if (transactionHolding == null)
                throw new ArgumentException("TranasctionHoldingID invalid");

            transactionHolding.DelToi = User.GetNameOrEmail();

            // Delete TransactionHolding
            await _transactionHoldingService.Delete(transactionHolding);

            return RedirectToAction(nameof(Batch), new { batchID = transactionHolding.BatchId }).WithSuccess("Transaction deleted", "");
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Batch), new { batchID }).WithDanger(ex.Message, "");
        }
    }

    #region Utilities

    [NonAction]
    private async Task<Customer> GetValidCustomer(string customerId)
    {
        // Make sure CustomerID has an integer value
        if (!int.TryParse(customerId, out int customerIdInt))
            throw new ArgumentException("CustomerID not an integer value");

        // Make sure CustomerID is valid
        var customer = await _customerService.GetById(customerIdInt);
        if (customer == null)
            throw new ArgumentException("CustomerID invalid");

        return customer;
    }

    [NonAction]
    private async Task<TransactionCode> GetValidTransactionCode(string transactionCodeId, int transactionCodeIdInt)
    {
        // Make sure TransactionCodeID has an integer value
        if (transactionCodeIdInt == 0 && !int.TryParse(transactionCodeId, out transactionCodeIdInt))
            throw new ArgumentException("TransactionCodeID not an integer value");

        // Make sure TransactionCodeID is valid
        var transactionCode = await _transactionCodeService.GetById(transactionCodeIdInt);
        if (transactionCode == null)
            throw new ArgumentException("TransactionCodeID invalid");

        return transactionCode;
    }

    [NonAction]
    private async Task<TransactionHolding> GetValidTransactionHolding(string transactionHoldingId, int transactionHoldingIdInt, int customerId)
    {
        // Make sure TransactionHoldingID has an integer value
        if (transactionHoldingIdInt == 0 && !int.TryParse(transactionHoldingId, out transactionHoldingIdInt))
            throw new ArgumentException("TransactionHoldingID not an integer value");

        // Make sure TransactionHoldingID is valid
        var transactionHolding = await _transactionHoldingService.GetById(transactionHoldingIdInt);
        if (transactionHolding == null)
            throw new ArgumentException("TransactionHoldingID invalid");

        // Make sure TransactionHoldingID matches CustomerID
        if (transactionHolding.CustomerId != customerId)
            throw new ArgumentException("TransactionHoldingID does not belong to given CustomerID");

        return transactionHolding;
    }

    [NonAction]
    private async Task<ServiceAddress> GetValidServiceAddress(string serviceAddressId, int serviceAddressIdInt, int customerId)
    {
        // Make sure ServiceAddressID has an integer value
        if (serviceAddressIdInt == 0 && !int.TryParse(serviceAddressId, out serviceAddressIdInt))
            throw new ArgumentException("ServiceAddressID not an integer value");

        // Make sure ServiceAddressID is valid
        var serviceAddress = await _serviceAddressService.GetById(serviceAddressIdInt);
        if (serviceAddress == null)
            throw new ArgumentException("ServiceAddressID invalid");

        // Make sure ServiceAddressID matches CustomerID
        if (serviceAddress.CustomerId != customerId)
            throw new ArgumentException("ServiceAddressID does not belong to given CustomerID");

        return serviceAddress;
    }

    [NonAction]
    private async Task<Container> GetValidContainer(string containerId, int containerIdInt, int serviceAddressId)
    {
        // Make sure ContainerID has an integer value
        if (containerIdInt == 0 && !int.TryParse(containerId, out containerIdInt))
            throw new ArgumentException("ContainerID not an integer value");

        // Make sure ContainerID is valid
        var container = await _containerService.GetById(containerIdInt);
        if (container == null)
            throw new ArgumentException("ContainerID invalid");

        // Make sure ContainerID matches ServiceAddressID
        if (container.ServiceAddressId != serviceAddressId)
            throw new ArgumentException("ContainerID does not belong to given ServiceAddressID");

        return container;
    }

    [NonAction]
    private async Task<IEnumerable<SelectListItem>> GenerateTransactionCodeSelectListGroupP()
    {
        var transactionCodes = await _transactionCodeService.GetAllByGroup("P");

        var codes = transactionCodes.Select(tc => new SelectListItem
        {
            Text = tc.Code.PadRight(5, ' ') + ": " + tc.Description,
            Value = tc.TransactionCodeId.ToString()
        }).ToList();
        codes.Insert(0, new SelectListItem { Text = "SELECT A TRANSACTION CODE", Value = "0" });

        return codes;
    }

    #endregion
}
