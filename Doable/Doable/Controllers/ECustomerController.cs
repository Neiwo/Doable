using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    public class ECustomerController : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Employee/Customers/Index.cshtml", Index);
        }
    }
}
