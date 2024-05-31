using Microsoft.AspNetCore.Mvc;

namespace Doable.Controllers
{
    public class AdminServicesController : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Admin/AdminServices/Index.cshtml");
        }
    }
}
