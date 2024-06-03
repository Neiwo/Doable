using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Doable.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult EmployeeDashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var totalTasks = _context.Tasklists.Count(t => t.AssignedTo == username);
            var pendingTasks = _context.Tasklists.Count(t => t.AssignedTo == username && t.Status == "Pending");
            var toReviewTasks = _context.Tasklists.Count(t => t.AssignedTo == username && t.Status == "To Review");
            var completedTasks = _context.Tasklists.Count(t => t.AssignedTo == username && t.Status == "Completed");

            ViewData["TotalTasks"] = totalTasks;
            ViewData["PendingTasks"] = pendingTasks;
            ViewData["ToReviewTasks"] = toReviewTasks;
            ViewData["CompletedTasks"] = completedTasks;

            return View();
        }
    }
}
