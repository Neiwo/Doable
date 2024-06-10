using System;
using Doable.Data;
using Doable.Models;
using Doable.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Doable.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;


        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
             
                var existingUserByUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(model);
                }

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

                TempData["RegisterSuccess"] = true;

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
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                    var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = user.Email }, Request.Scheme);

                    var message = $"<p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>";
                    await _emailService.SendEmailAsync(user.Email, "Reset Password", message);

                    TempData["ForgotPasswordSuccess"] = true;
                }
                else
                {
                    TempData["ForgotPasswordError"] = "If the email exists in our system, you will receive a password reset link.";
                }

                return View();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    user.Password = model.NewPassword;  
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["ResetPasswordSuccess"] = true;
                    return View(model);
                }

                TempData["ResetPasswordError"] = "Error resetting your password. Please try again.";
            }

            return View(model);
        }
    }
}
