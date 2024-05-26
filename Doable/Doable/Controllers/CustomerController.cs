using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action to list customers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customers = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();
            return View("/Views/Admin/Customer/Index.cshtml", customers);
        }

        // Action to create customer
        [HttpGet]
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View("/Views/Admin/Customer/Create.cshtml", new User { Role = "Client" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already taken.");
                    return View("/Views/Admin/Customer/Create.cshtml", user);
                }

                user.CreatedBy = HttpContext.Session.GetString("Username");
                user.CreationDate = DateTime.Now;

                // Ensure the Role is set to Client
                user.Role = "Client";

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Admin/Customer/Create.cshtml", user);
        }
        // Action to edit customer
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Users.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View("/Views/Admin/Customer/Edit.cshtml", customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email && u.ID != user.ID))
                {
                    ModelState.AddModelError("Email", "Email address is already taken.");
                    return View("/Views/Admin/Customer/Edit.cshtml", user);
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Admin/Customer/Edit.cshtml", user);
        }

        // Action to delete customer
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Users.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View("/Views/Admin/Customer/Delete.cshtml", customer);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Users.FindAsync(id);
            _context.Users.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}