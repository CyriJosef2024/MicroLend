using Microsoft.AspNetCore.Mvc;

namespace MicroLend.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
