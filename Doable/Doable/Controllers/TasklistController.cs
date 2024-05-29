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
    public class TaskListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskList
        public async Task<IActionResult> Index(string searchString)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tasks = from t in _context.Tasklists
                        select t;

            if (!String.IsNullOrEmpty(searchString))
            {
                tasks = tasks.Where(t =>
                t.Title.Contains(searchString) ||
                t.Description.Contains(searchString) ||
                t.AssignedTo.Contains(searchString) ||
                t.Priority.Contains(searchString) ||
                t.Status.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            return View("/Views/Admin/TaskList/Index.cshtml", await tasks.ToListAsync());
        }

        // GET: TaskList/Create
        public async Task<IActionResult> Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Employees = await _context.Users
                .Where(u => u.Role == "Employee")
                .ToListAsync();

            return View("/Views/Admin/TaskList/Create.cshtml");
        }

        // POST: TaskList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Description,AssignedTo,CreatedBy,Priority,Status,CreatedDate,Deadline")] Tasklist tasklist)
        {
            if (ModelState.IsValid)
            {
                tasklist.CreatedDate = DateTime.Now; // Set the created date to now
                _context.Add(tasklist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = await _context.Users
                .Where(u => u.Role == "Employee")
                .ToListAsync();

            return View("/Views/Admin/TaskList/Create.cshtml", tasklist);
        }

        // GET: TaskList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var tasklist = await _context.Tasklists.FindAsync(id);
            if (tasklist == null)
            {
                return NotFound();
            }

            ViewBag.Employees = await _context.Users
                .Where(u => u.Role == "Employee")
                .ToListAsync();

            return View("/Views/Admin/TaskList/Edit.cshtml", tasklist);
        }

        // POST: TaskList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Description,AssignedTo,CreatedBy,Priority,Status,CreatedDate,Deadline")] Tasklist tasklist)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id != tasklist.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tasklist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TasklistExists(tasklist.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = await _context.Users
                .Where(u => u.Role == "Employee")
                .ToListAsync();

            return View("/Views/Admin/TaskList/Edit.cshtml", tasklist);
        }

        // GET: TaskList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var tasklist = await _context.Tasklists
                .FirstOrDefaultAsync(m => m.ID == id);
            if (tasklist == null)
            {
                return NotFound();
            }

            return View("/Views/Admin/TaskList/Delete.cshtml", tasklist);
        }

        // POST: TaskList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var tasklist = await _context.Tasklists.FindAsync(id);
            _context.Tasklists.Remove(tasklist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TasklistExists(int id)
        {
            return _context.Tasklists.Any(e => e.ID == id);
        }
    }
}
