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
    [Route("admin/team")]
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context)
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
                    u.Role.Contains(searchString) ||
                    u.CreatedBy.Contains(searchString) ||
                    u.CreationDate.ToString().Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            int totalUsers = await users.CountAsync();
            var usersOnPage = await users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["Users"] = usersOnPage;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View("/Views/Admin/Team/Index.cshtml");
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View("/Views/Admin/Team/Create.cshtml", new User());
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already taken.");
                    return View("/Views/Admin/Team/Create.cshtml", user);
                }

                user.CreatedBy = HttpContext.Session.GetString("Username");
                user.CreationDate = DateTime.Now;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Admin/Team/Create.cshtml", user);
        }


        [HttpGet("edit/{id}")]
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
            return View("/Views/Admin/Team/Edit.cshtml", user);
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
                    return View("/Views/Admin/Team/Edit.cshtml", user);
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Admin/Team/Edit.cshtml", user);
        }

        [HttpGet("delete/{id}")]
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
            return View("/Views/Admin/Team/Delete.cshtml", user);
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
