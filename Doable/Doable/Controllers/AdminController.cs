using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminDashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Use userId as needed
            return View();
        }

        public IActionResult Tasks()
        {
            return RedirectToAction("Index", "TaskList");
        }

        // Action to list users with pagination and search
        [HttpGet]
        public async Task<IActionResult> Team(string searchString, int pageNumber = 1, int pageSize = 6)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var users = from u in _context.Users
                        where u.Role == "Admin" || u.Role == "Employee"
                        select u;

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u =>
                    u.ID.ToString().Contains(searchString) ||
                    u.Username.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.PhoneNumber.ToString().Contains(searchString) ||
                    u.CreatedBy.Contains(searchString) ||
                    u.CreationDate.ToString().Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            int totalUsers = await users.CountAsync();
            var usersOnPage = await users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["Users"] = usersOnPage;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View();
        }

        // Action to create user
        [HttpGet]
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(new User());
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already taken.");
                    return View(user);
                }

                user.CreatedBy = HttpContext.Session.GetString("Username");
                user.CreationDate = DateTime.Now;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Team));
            }
            return View(user);
        }

        // Action to edit user
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
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
                    return View(user);
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Team));
            }
            return View(user);
        }

        // Action to delete user
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Team));
        }
    }
}
