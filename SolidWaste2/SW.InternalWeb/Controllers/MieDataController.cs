using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SW.BLL.Services;
using SW.DM;
using SW.InternalWeb.Models.MieData;

namespace SW.InternalWeb.Controllers
{
    public class MieDataController : Controller
    {
        private readonly IMieDataService mieDataService;
        private readonly ICustomerService customerService;

        public MieDataController(IMieDataService mieDataService, ICustomerService customerService)
        {
            this.mieDataService = mieDataService;
            this.customerService = customerService;
        }

        public IActionResult Index(int? customerId)
        {
            MieDataListViewModel vm = new()
            {
                CustomerId = customerId
            };
            return View(vm);
        }

        public async Task<IActionResult> Edit(MieDataListViewModel vm, IFormFile[] image)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Customer", new { vm.CustomerId })
                    .WithDanger("Upload Image Failed", "Model State Error");
            if (image == null || !image.Any())
                return RedirectToAction("Index", "Customer", new { vm.CustomerId })
                    .WithDanger("File Upload Failed", "No file selected");

            await mieDataService.Deactivate(vm.CustomerId.Value, User.GetNameOrEmail());

            foreach(var i in image)
            {
                if (i == null)
                    continue;

                var fileLength = (int)i.Length;
                var bytes = new byte[fileLength];
                using (var stream = i.OpenReadStream())
                {
                    await stream.ReadAsync(bytes.AsMemory(0, fileLength));
                }

                var mieData = new MieData
                {
                    AddDateTime = DateTime.Now,
                    AddToi = User.GetNameOrEmail(),
                    MieDataActive = true,

                    MieDataImage = bytes,
                    MieDataImageFileName = i.FileName,
                    MieDataImageId = vm.CustomerId.ToString(),
                    MieDataImageSize = fileLength,
                    MieDataImageType = i.ContentType
                };
                await mieDataService.Add(mieData);
            }

            return RedirectToAction("Index", "Customer", new { vm.CustomerId })
                .WithSuccess("File uploaded", "");
        }

        public IActionResult Return(MieDataListViewModel vm, IFormFile[] image)
        {
            return RedirectToAction("Index", "Customer", new { vm.CustomerId });
        }

        public async Task<IActionResult> ImageDownload(int id, int customerID)
        {
            var image = await mieDataService.GetById(id);
            if (image == null || image.DelDateTime != null)
                return RedirectToAction("Index", "Customer", new { customerID })
                    .WithDanger("No file found", "");

            return File(image.MieDataImage, image.MieDataImageType, image.MieDataImageFileName);
        }

        public async Task<IActionResult> ImageDelete(int id, int customerID)
        {
            await mieDataService.Delete(id);
            return RedirectToAction("Index", "Customer", new { customerID })
                .WithSuccess("Delete Successful", "");
        }

        public async Task<IActionResult> ImageInactive(int id)
        {
            var customer = await customerService.GetById(id);
            if(customer != null)
            {
                await mieDataService.Deactivate(id, User.GetNameOrEmail());

                customer.ChgDateTime = DateTime.Now;
                customer.ChgToi = User.GetNameOrEmail();
                customer.ContractCharge = null;
                await customerService.Update(customer);
            }

            return RedirectToAction("Index", "Customer", new { customerId = id });
        }
    }
}
