using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    [Route("admin/customers")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1, int pageSize = 6)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customers = from c in _context.Users
                            where c.Role == "Client"
                            select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.ID.ToString().Contains(searchString) ||
                    c.Username.Contains(searchString) ||
                    c.Email.Contains(searchString) ||
                    c.PhoneNumber.ToString().Contains(searchString) ||
                    c.CreatedBy.Contains(searchString) ||
                    c.CreationDate.ToString().Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            int totalCustomers = await customers.CountAsync();
            var customersOnPage = await customers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new CustomerListViewModel
            {
                Customers = customersOnPage,
                Pagination = new Pagination
                {
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalCustomers / (double)pageSize)
                }
            };

            return View("/Views/Admin/Customer/Index.cshtml", viewModel);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View("/Views/Admin/Customer/Create.cshtml", new User { Role = "Client" });
        }

        [HttpPost("create")]
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

                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View("/Views/Admin/Customer/Create.cshtml", user);
                }

                user.CreatedBy = HttpContext.Session.GetString("Username");
                user.CreationDate = DateTime.Now;

                user.Role = "Client";

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Admin/Customer/Create.cshtml", user);
        }

        [HttpGet("edit/{id}")]
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

        [HttpPost("edit/{id}")]
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

        [HttpGet("delete/{id}")]
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

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Users.FindAsync(id);
            _context.Users.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public class CustomerListViewModel
        {
            public IEnumerable<User> Customers { get; set; }
            public Pagination Pagination { get; set; }
        }

        public class Pagination
        {
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
