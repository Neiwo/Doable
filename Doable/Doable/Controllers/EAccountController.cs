using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    public class EAccountController : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Employee/Account/Index.cshtml", Index);
        }
    }
}
