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
    public class CTaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CTaskController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string status, int pageNumber = 1, int pageSize = 6)
        {
            try
            {
                string username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Query tasks created by the user and include members
                var tasks = _context.Tasklists
                    .Where(t => t.CreatedBy == username)
                    .Include(t => t.Members)
                    .AsQueryable();

                if (!tasks.Any())
                {
                    ViewData["NoTasksMessage"] = "You have no Tickets or Concerns.";
                }

                if (!string.IsNullOrEmpty(status))
                {
                    tasks = tasks.Where(t => t.Status == status);
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

                // Always sort by CreatedDate in descending order
                tasks = tasks.OrderByDescending(t => t.CreatedDate);

                ViewData["CurrentFilter"] = searchString;

                int totalTasks = await tasks.CountAsync();
                var tasksOnPage = await tasks.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                var viewModel = new CTaskViewModel
                {
                    Tasks = tasksOnPage,
                    Pagings = new Pagination
                    {
                        CurrentPage = pageNumber,
                        TotalPages = (int)Math.Ceiling(totalTasks / (double)pageSize)
                    }
                };

                return View("/Views/Client/Task/Index.cshtml", viewModel);
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
                return View("/Views/Client/Task/Create.cshtml", new Tasklist());
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
                    // Get the current user's username to set as the creator
                    var createdBy = HttpContext.Session.GetString("Username") ?? "Unknown User";
                    task.CreatedBy = createdBy;

                    _context.Tasklists.Add(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Employees = _context.Users.Select(u => new { u.Username }).ToList();
                return View("/Views/Client/Task/Create.cshtml", task);
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
                return View("/Views/Client/Task/Edit.cshtml", task);
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
                return View("/Views/Client/Task/Edit.cshtml", task);
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

                return View("/Views/Client/Task/Delete.cshtml", task);
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

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var task = await _context.Tasklists
                    .Include(t => t.Notes)
                    .Include(t => t.Docus)
                    .FirstOrDefaultAsync(t => t.ID == id);

                if (task == null)
                {
                    return NotFound();
                }

                var username = HttpContext.Session.GetString("Username");
                var canEdit = task.Status != "Completed" && task.AssignedTo == username;

                // Allow members to upload files if the status is not "Completed"
                var canUploadFiles = task.Status != "Completed";

                var adminFiles = task.Docus.Where(d => d.UploadedbyRole == "Admin").ToList();
                var employeeFiles = task.Docus.Where(d => d.UploadedbyRole == "Employee").ToList();
                var clientFiles = task.Docus.Where(d => d.UploadedbyRole == "Client").ToList(); // New line for Client files

                var viewModel = new TaskDetailsViewModel
                {
                    Task = task,
                    CanEdit = canEdit,
                    CanUploadFiles = canUploadFiles,
                    AdminFiles = adminFiles,
                    EmployeeFiles = employeeFiles,
                    ClientFiles = clientFiles // Add Client files to the view model
                };

                return View("/Views/Client/Task/Details.cshtml", viewModel);
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
            return View("/Views/Client/Task/AddFiles.cshtml");
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

                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                var docu = new Docu
                {
                    TasklistID = tasklistId,
                    FileName = file.FileName,
                    FilePath = filePath,
                    UploadedDate = DateTime.Now,
                    Uploadedby = username,
                    UploadedbyRole = user?.Role // Set the role of the uploader
                };

                _context.Docus.Add(docu);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = tasklistId });
            }

            ViewBag.TasklistID = tasklistId;
            return View("/Views/Client/Task/AddFiles.cshtml");
        }
    }

    public class CTaskViewModel
    {
        public IEnumerable<Tasklist> Tasks { get; set; }
        public Pagination Pagings { get; set; }
    }

    public class Pagings
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
