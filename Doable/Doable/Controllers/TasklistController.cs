using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
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

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1, int pageSize = 6)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var tasks = from t in _context.Tasklists
                            select t;

                if (!string.IsNullOrEmpty(searchString))
                {
                    tasks = tasks.Where(t =>
                        t.Title.Contains(searchString) ||
                        t.Description.Contains(searchString) ||
                        t.AssignedTo.Contains(searchString) ||
                        t.Priority.Contains(searchString) ||
                        t.Status.Contains(searchString) ||
                        t.CreatedDate.ToString().Contains(searchString) ||
                        t.Deadline.ToString().Contains(searchString));
                }

                ViewData["CurrentFilter"] = searchString;

                int totalTasks = await tasks.CountAsync();
                var tasksOnPage = await tasks.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                var viewModel = new TaskListViewModel
                {
                    Tasks = tasksOnPage,
                    Pagination = new Pagination
                    {
                        CurrentPage = pageNumber,
                        TotalPages = (int)Math.Ceiling(totalTasks / (double)pageSize)
                    }
                };

                return View("/Views/Admin/TaskList/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Employees = _context.Users
                    .Where(u => u.Role == "Employee")
                    .Select(u => new { u.Username })
                    .ToList();
                return View("/Views/Admin/TaskList/Create.cshtml", new Tasklist());
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tasklist task)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Tasklists.Add(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Employees = _context.Users.Select(u => new { u.Username }).ToList();
                return View("/Views/Admin/TaskList/Create.cshtml", task);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                ViewBag.Employees = _context.Users
                    .Where(u => u.Role == "Employee")
                    .Select(u => new { u.Username })
                    .ToList();

                return View("/Views/Admin/TaskList/Edit.cshtml", task);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Tasklist task)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Employees = _context.Users.Select(u => new { u.Username }).ToList();
                return View("/Views/Admin/TaskList/Edit.cshtml", task);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                return View("/Views/Admin/TaskList/Delete.cshtml", task);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                _context.Tasklists.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNotes(int id)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                ViewBag.TaskId = id;
                return View("/Views/Admin/TaskList/AddNotes.cshtml");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNotes(int id, string remarks)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                var note = new Notes
                {
                    TaskID = id,
                    Remarks = remarks,
                };

                _context.Notes.Add(note);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var task = await _context.Tasklists
                    .Include(t => t.Notes)
                    .FirstOrDefaultAsync(t => t.ID == id);
                if (task == null)
                {
                    return NotFound();
                }

                return View("/Views/Admin/TaskList/Details.cshtml", task);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                task.Status = "Completed"; // Update the status to Completed
                _context.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Return(int id)
        {
            try
            {
                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                task.Status = "Returned"; // Update the status to Returned
                _context.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditNote(int id)
        {
            try
            {
                var note = await _context.Notes.FindAsync(id);
                if (note == null)
                {
                    return NotFound();
                }

                return View("/Views/Admin/TaskList/EditNote.cshtml", note);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditNote(Notes note)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = note.TaskID });
                }
                return View("/Views/Admin/TaskList/EditNote.cshtml", note);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

    }

    public class TaskListViewModel
    {
        public IEnumerable<Tasklist> Tasks { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
