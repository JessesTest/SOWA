using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.TransactionCode;

namespace SW.InternalWeb.Controllers;

public class TransactionCodeController : Controller
{
    private readonly ITransactionCodeService _transactionCodeService;

    public TransactionCodeController(ITransactionCodeService transactionCodeService)
    {
        _transactionCodeService = transactionCodeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var transactionCodes = await _transactionCodeService.GetAll();

        var vm = transactionCodes.Select(tc => new TransactionCodeListViewModel
        {
            TransactionCodeID = tc.TransactionCodeId,
            Code = tc.Code,
            Description = tc.Description,
            TransactionSign = tc.TransactionSign,
            CollectionsBalanceSign = tc.CollectionsBalanceSign,
            CounselorsBalanceSign = tc.CounselorsBalanceSign,
            //UncollectableBalanceSign = tc.UncollectableBalanceSign,
            AccountType = tc.AccountType,
            Group = tc.Group
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var code = await _transactionCodeService.GetById(id);
        if (code == null)
            return RedirectToAction(nameof(Index)).WithDanger("[TransactionCodeId] invalid", "");

        var vm = new TransactionCodeEditViewModel
        {
            TransactionCodeID = code.TransactionCodeId,
            Code = code.Code,
            Description = code.Description,
            TransactionSign = code.TransactionSign,
            CollectionsBalanceSign = code.CollectionsBalanceSign,
            CounselorsBalanceSign = code.CounselorsBalanceSign,
            UncollectableBalanceSign = code.UncollectableBalanceSign,
            AccountType = code.AccountType
            //Group = code.Group
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TransactionCodeEditViewModel vm)
    {
        try
        {
            var code = await _transactionCodeService.GetById(vm.TransactionCodeID);
            if (code == null)
                throw new ArgumentException("[TransactionCodeId] invalid");

            code.Code = vm.Code;
            code.Description = vm.Description;
            code.TransactionSign = vm.TransactionSign;
            code.CollectionsBalanceSign = vm.CollectionsBalanceSign;
            code.CounselorsBalanceSign = vm.CounselorsBalanceSign;
            code.UncollectableBalanceSign = vm.UncollectableBalanceSign;
            code.AccountType = vm.AccountType;
            //code.Group = vm.Group;
            code.ChgToi = User.GetNameOrEmail();

            await _transactionCodeService.Update(code);

            return RedirectToAction("Edit", new { id = vm.TransactionCodeID }).WithSuccess("Update Successful", "");
        }
        catch (Exception ex)
        {
            return View(vm).WithDanger(ex.Message, "");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var code = await _transactionCodeService.GetById(id);
            if (code == null)
                throw new ArgumentException("[TransactionCodeId] invalid");

            code.DelToi = User.GetNameOrEmail();

            await _transactionCodeService.Delete(code);

            return RedirectToAction(nameof(Index)).WithSuccess("Delete Successful", "");
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Index)).WithDanger(ex.Message, "");
        }
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View(new TransactionCodeAddViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Add(TransactionCodeAddViewModel vm)
    {
        try
        {
            var code = new TransactionCode
            {
                Code = vm.Code,
                Description = vm.Description,
                TransactionSign = vm.TransactionSign,
                CollectionsBalanceSign = vm.CollectionsBalanceSign,
                CounselorsBalanceSign = vm.CounselorsBalanceSign,
                UncollectableBalanceSign = vm.UncollectableBalanceSign,
                AccountType = vm.AccountType,
                //Group = vm.Group,
                AddToi = User.GetNameOrEmail()
            };

            await _transactionCodeService.Add(code);

            return RedirectToAction(nameof(Edit), new { id = code.TransactionCodeId }).WithSuccess("Add Successful", "");
        }
        catch (Exception ex)
        {
            return View(vm).WithDanger(ex.Message, "");
        }
    }
}
