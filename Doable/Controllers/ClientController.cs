using Microsoft.AspNetCore.Mvc;

namespace Doable.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult ClientDashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Use userId as needed
            return View();
        }
    }
}