using Microsoft.AspNetCore.Mvc;

namespace AquaDex.Api.wwwroot.css
{
    public class theme : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
