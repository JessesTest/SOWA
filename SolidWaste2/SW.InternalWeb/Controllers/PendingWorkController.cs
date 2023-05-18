using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Notify.BL.Services;
using Notify.DM;
using SW.BLL.Services;
using SW.InternalWeb.Models.PendingWork;

namespace SW.InternalWeb.Controllers;

public class PendingWorkController : Controller
{
    private readonly ITransactionHoldingService transactionHoldingService;
    private readonly INotifyService notifyService;

    public PendingWorkController(
        ITransactionHoldingService transactionHoldingService,
        INotifyService notifyService)
    {
        this.transactionHoldingService = transactionHoldingService;
        this.notifyService = notifyService;
    }

    public async Task<IActionResult> All()
    {
        var holdings = await transactionHoldingService.GetAll();
        PendingWorkViewModel model = new(holdings);
        return View("List", model);
    }

    public async Task<IActionResult> Single(int transactionHoldingID)
    {
        var email = User.GetEmail(); // ?? "Angie.Orester@snco.us"
        var holding = await transactionHoldingService.GetAuthorizedById(email, transactionHoldingID);
        if(holding == null)
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("Transaction holding not found", "Record does not exist or you are not authorized");
        }

        var temp = new[] { holding };
        var model = new PendingWorkViewModel(temp);
        return View("List", model);
    }

    public async Task<IActionResult> Personal()
    {
        var email = User.GetEmail(); // ?? "Angie.Orester@snco.us"
        var holdings = await transactionHoldingService.GetAllAuthorized(email);
        PendingWorkViewModel model = new(holdings);
        return View("List", model);
    }

    public async Task<IActionResult> ForceResolve(int transactionHoldingID)
    {
        var email = User.GetEmail(); // ?? "Deanna.Starkebaum@snco.us"
        var holding = await transactionHoldingService.GetAuthorizedById(email, transactionHoldingID);
        if(holding == null)
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("Transaction holding not found", "Record does not exist or you are not authorized");
        }

        var displayName = User.GetDisplayName() ?? "Starkebaum, Deanna";
        //var accountName = User.GetNameOrEmail()

        string result = await transactionHoldingService.Resolve(transactionHoldingID, email, displayName);
        if(result != null)
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("", result);
        }

        return RedirectToAction("Index", "CustomerTransactionHistory", new { customerId = holding.CustomerId })
            .WithSuccess("Success", "Transaction resolved");
    }

    public async Task<IActionResult> ApproveTransaction(int transactionHoldingID)
    {
        var email = User.GetEmail(); // ?? "Deanna.Starkebaum@snco.us"
        var holding = await transactionHoldingService.GetAuthorizedById(email, transactionHoldingID);
        if (holding == null)
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("Transaction holding not found", "Record does not exist or you are not authorized");
        }

        var displayName = User.GetDisplayName() ?? "Starkebaum, Deanna";
        //var accountName = User.GetNameOrEmail()

        string result = await transactionHoldingService.Approve(transactionHoldingID, email, displayName);
        if (result != null)
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("", result);
        }

        return RedirectToAction("Index", "CustomerTransactionHistory", new { customerId = holding.CustomerId })
            .WithSuccess("Success", "Transaction resolved");
    }

    public async Task<IActionResult> RejectTransaction(RejectTransactionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(nameof(Personal))
                .WithWarning("", "There are errors on the form");

        var email = User.GetEmail(); // ?? "Deanna.Starkebaum@snco.us"
        var holding = await transactionHoldingService.GetAuthorizedById(email, model.TransactionHoldingID.Value);
        if (holding == null)
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("Transaction holding not found", "Record does not exist or you are not authorized");
        }

        var displayName = User.GetDisplayName() ?? "Starkebaum, Deanna";
        //var accountName = User.GetNameOrEmail()

        string result = await transactionHoldingService.Reject(model.TransactionHoldingID.Value, model.Comment, email, displayName);
        if (!string.IsNullOrWhiteSpace(result))
        {
            return RedirectToAction(nameof(Personal))
                .WithWarning("", result);
        }

        var notifyBody = $@"
<p>Transaction Rejected<br />
Details:<br />
&nbsp;&nbsp;TransactionHoldingID: {holding.TransactionCodeId}<br />
&nbsp;&nbsp;CustomerID: {holding.CustomerId}<br />
&nbsp;&nbsp;SubmitDate: {holding.AddDateTime}<br />
&nbsp;&nbsp;SubmittedBy: {holding.Sender}<br />
&nbsp;&nbsp;TransactionCode: {holding.TransactionCode.Code} - {holding.TransactionCode.Description}<br />
&nbsp;&nbsp;TransactionAmt: {holding.TransactionAmt}<br />
Reason:<br />
&nbsp;&nbsp;{model.Comment}</p>";

        // add notification
        Notification notification = new()
        {
            AddDateTime = DateTime.Now,
            Body = notifyBody,
            From = email,
            To = holding.Sender,
            Subject = "Transaction Rejected",
            System = "Solid Waste"
        };
        await notifyService.Add(notification);

        return RedirectToAction(nameof(Personal))
            .WithSuccess("Success", "Transaction rejected");
    }
}
