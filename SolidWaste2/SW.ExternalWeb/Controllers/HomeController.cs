using Microsoft.AspNetCore.Mvc;

namespace SW.ExternalWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
