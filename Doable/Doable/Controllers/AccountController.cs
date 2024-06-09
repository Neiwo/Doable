using System;
using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.ID);
                    HttpContext.Session.SetString("Username", user.Username);

                    TempData["LoginSuccess"] = true;

                    if (user.Role == "Admin")
                    {
                        TempData["RedirectUrl"] = Url.Action("AdminDashboard", "Admin");
                    }
                    else if (user.Role == "Employee")
                    {
                        TempData["RedirectUrl"] = Url.Action("EmployeeDashboard", "Employee");
                    }
                    else if (user.Role == "Client")
                    {
                        TempData["RedirectUrl"] = Url.Action("ClientDashboard", "Client");
                    }

                    return View();
                }
                ModelState.AddModelError("Username", "Incorrect Username or Password");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username is already taken
                var existingUserByUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(model);
                }

                // Check if the email is already taken
                var existingUserByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    Role = model.Role,
                    PhoneNumber = model.PhoneNumber,
                    CreatedBy = "Registration",
                    CreationDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["RegisterSuccess"] = true; // Set success flag

                return RedirectToAction("Register");
            }

            return View(model);
        }



        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
