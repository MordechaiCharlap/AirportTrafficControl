using Microsoft.AspNetCore.Mvc;

namespace AirportTrafficControlTower.Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
