using Microsoft.AspNetCore.Mvc;

namespace Doable.Controllers
{
    public class RentalsController : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Admin/Rentals/Index.cshtml");
        }
    }
}
