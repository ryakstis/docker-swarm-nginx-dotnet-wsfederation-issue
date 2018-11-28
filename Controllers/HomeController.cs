using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wsfed_issue.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
