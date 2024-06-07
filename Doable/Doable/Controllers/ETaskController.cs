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
    public class ETaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ETaskController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1, int pageSize = 6)
        {
            try
            {
                string username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Query tasks assigned to the user
                var assignedTasks = _context.Tasklists
                    .Where(t => t.AssignedTo == username && t.Status != "To Review" && t.Status != "Completed")
                    .Include(t => t.Members);

                // Query tasks where the user is a member
                var memberTasks = _context.Tasklists
                    .Where(t => t.Members.Any(m => m.Username == username) && t.Status != "To Review" && t.Status != "Completed")
                    .Include(t => t.Members);

                // Combine both queries
                var tasks = assignedTasks
                    .Union(memberTasks);

                if (!tasks.Any())
                {
                    ViewData["NoTasksMessage"] = "There are no tasks currently available.";
                }

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

                var viewModel = new ETaskViewModel
                {
                    Tasks = tasksOnPage,
                    Paging = new Pagination
                    {
                        CurrentPage = pageNumber,
                        TotalPages = (int)Math.Ceiling(totalTasks / (double)pageSize)
                    }
                };

                return View("/Views/Employee/Task/Index.cshtml", viewModel);
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
                string username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Employees = _context.Users
                    .Where(u => u.Role == "Employee")
                    .Select(u => new { u.Username })
                    .ToList();
                return View("/Views/Employee/Task/Create.cshtml", new Tasklist());
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
                return View("/Views/Employee/Task/Create.cshtml", task);
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

                ViewBag.Employees = _context.Users.Select(u => new { u.Username }).ToList();
                return View("/Views/Employee/Task/Edit.cshtml", task);
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
                return View("/Views/Employee/Task/Edit.cshtml", task);
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

                return View("/Views/Employee/Task/Delete.cshtml", task);
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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var task = await _context.Tasklists
                    .Include(t => t.Notes)
                    .Include(t => t.Docus) // Include the Docus navigation property
                    .FirstOrDefaultAsync(t => t.ID == id);

                if (task == null)
                {
                    return NotFound();
                }

                if (task.Status != "Opened")
                {
                    task.Status = "Opened";
                    _context.Tasklists.Update(task);
                    await _context.SaveChangesAsync();
                }

                return View("/Views/Employee/Task/Details.cshtml", task);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }



        [HttpPost]
        public async Task<IActionResult> CompleteTask(int id)
        {
            try
            {
                var task = await _context.Tasklists.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                task.Status = "To Review";
                _context.Tasklists.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        [HttpGet]
        public IActionResult AddFiles(int tasklistId)
        {
            ViewBag.TasklistID = tasklistId;
            return View("/Views/Employee/Task/AddFiles.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> AddFiles(int tasklistId, IFormFile file)
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var docu = new Docu
                {
                    TasklistID = tasklistId,
                    FileName = file.FileName,
                    FilePath = filePath,
                    UploadedDate = DateTime.Now,
                    Uploadedby = username // Save the uploader's username
                };

                _context.Docus.Add(docu);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = tasklistId });
            }

            ViewBag.TasklistID = tasklistId;
            return View("/Views/Employee/Task/AddFiles.cshtml");
        }



    }

    public class ETaskViewModel
    {
        public IEnumerable<Tasklist> Tasks { get; set; }
        public Pagination Paging { get; set; }
    }

    public class Paging
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}