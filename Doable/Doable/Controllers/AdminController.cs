using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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

            var totalTasks = _context.Tasklists.Count();
            var pendingTasks = _context.Tasklists.Count(t => t.Status == "Pending");
            var toReviewTasks = _context.Tasklists.Count(t => t.Status == "To Review");
            var completedTasks = _context.Tasklists.Count(t => t.Status == "Completed");

            ViewData["TotalTasks"] = totalTasks;
            ViewData["PendingTasks"] = pendingTasks;
            ViewData["ToReviewTasks"] = toReviewTasks;
            ViewData["CompletedTasks"] = completedTasks;

            var currentDate = DateTime.Now;
            var currentBookings = _context.Bookings.Where(b => b.ServiceDateFrom <= currentDate && b.ServiceDateTo >= currentDate).ToList();
            var pastBookings = _context.Bookings.Where(b => b.ServiceDateTo < currentDate).ToList();

            ViewData["CurrentBookings"] = currentBookings;
            ViewData["PastBookings"] = pastBookings;

            return View();
        }
    }
}
