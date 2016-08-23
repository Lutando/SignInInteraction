using Microsoft.AspNetCore.Mvc;

namespace Identity.Sts.UI.Home
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}