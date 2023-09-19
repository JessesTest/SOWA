using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;

namespace SW.InternalWeb.Controllers;

public class DelinquenciesController : Controller
{
    private readonly ITransactionService transactionService;

    public DelinquenciesController(
        ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await transactionService.GetAllDelinquencies();
        return View(model);
    }
}
