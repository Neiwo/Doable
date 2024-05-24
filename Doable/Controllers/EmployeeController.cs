using Microsoft.AspNetCore.Mvc;

namespace Doable.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult EmployeeDashboard()
        {
            return View();
        }
    }
}