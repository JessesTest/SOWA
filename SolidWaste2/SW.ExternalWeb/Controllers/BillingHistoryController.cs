using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PE.BL.Services;
using SW.BLL.Services;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.BillingHistory;

namespace SW.ExternalWeb.Controllers;

[Authorize]
public class BillingHistoryController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IPersonEntityService personEntityService;
    private readonly ICustomerService customerService;
    private readonly ITransactionService transactionService;
    private readonly IBillBlobService billBlobService;

    public BillingHistoryController(
        UserManager<ApplicationUser> userManager,
        IPersonEntityService personEntityService,
        ICustomerService customerService,
        ITransactionService transactionService,
        IBillBlobService billBlobService)
    {
        this.userManager = userManager;
        this.personEntityService = personEntityService;
        this.customerService = customerService;
        this.transactionService = transactionService;
        this.billBlobService = billBlobService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await userManager.FindByIdAsync(User.GetUserId());
        var person = await personEntityService.GetById(user.UserId);
        var customer = await customerService.GetByPE(person.Id);
        var customerId = customer.CustomerId;

        var listing = await transactionService.GetListingByCustomer(customerId, false);

        BillingHistoryViewModel model = new()
        {
            CustomerID = customerId.ToString(),
            Transactions = listing.Select(e => new BillingHistoryListViewModel
            {
                AddDateTime = e.AddDateTime,
                BalanceForward = (e.TransactionBalance - e.TransactionAmount).ToString("c"),
                Description = ListingDescription(e.TransactionCode, e.TransactionComment, e.CodeDescription),
                TransactionAmount = e.TransactionAmount.ToString("c"),
                TransactionBalance = e.TransactionBalance.ToString("c"),
                TransactionCode = e.TransactionCode,
                TransactionID = e.TransactionID.ToString()
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<ActionResult> DownloadBill(int TransactionID)
    {
        var user = await userManager.FindByIdAsync(User.GetUserId());
        var person = await personEntityService.GetById(user.UserId);
        var customer = await customerService.GetByPE(person.Id);
        var customerId = customer.CustomerId;

        var bill_blob_record = await billBlobService.GetByTransactionId(TransactionID);

        if (bill_blob_record.CustomerId != customerId)
        {
            return BadRequest("Invalid account! Unable to view customer bill.");
        }

        return File(bill_blob_record.BillFile, "application/pdf", "sw_bill.pdf");
    }



    [NonAction]
    private string ListingDescription(string transactionCode, string transactionComment, string codeDescription)
    {
        string temp;
        if (transactionCode == null || transactionCode == "MB")
            temp = transactionComment ?? "";
        else
            temp =  codeDescription ?? "";

        if(temp == "PAYMENT")
            temp += " - Thank You!";

        return temp;
    }
}
