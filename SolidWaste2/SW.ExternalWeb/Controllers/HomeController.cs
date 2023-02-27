using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SW.ExternalWeb.Models;
using System.Diagnostics;

namespace SW.ExternalWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MakeError()
        {
            logger.LogError("MakeError");
            throw new NotImplementedException("Just testing");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            //this is the error that would have been redirected here by the global handler
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                logger.LogError("{userName} experienced {errorMessage} during transaction {transactionId}", User.GetNameOrEmail(), exceptionFeature.Error.Message, Activity.Current?.Id);
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
