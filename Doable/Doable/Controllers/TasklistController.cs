using Doable.Data;
using Doable.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Doable.Controllers
{
    [Route("Admin/[controller]/[action]")]
    public class TasklistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasklistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Tasklist/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var tasks = await _context.Tasklists.ToListAsync();
                return View("/Views/Admin/Tasklist/Index.cshtml", tasks);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        // GET: Admin/Tasklist/Create
        public IActionResult Create()
        {
            return View("/Views/Admin/Tasklist/Create.cshtml");
        }

        // POST: Admin/Tasklist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Description,CreatedBy,AssignedTo,Priority,Status,CreatedDate,Deadline")] Tasklist tasklist)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tasklist.CreatedDate = DateTime.Now;
                    _context.Add(tasklist);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                    return View("Error");
                }
            }
            return View("/Views/Admin/Tasklist/Create.cshtml", tasklist);
        }

        // GET: Admin/Tasklist/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasklist = await _context.Tasklists.FindAsync(id);
            if (tasklist == null)
            {
                return NotFound();
            }
            return View("/Views/Admin/Tasklist/Edit.cshtml", tasklist);
        }

        // POST: Admin/Tasklist/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Description,CreatedBy,AssignedTo,Priority,Status,CreatedDate,Deadline")] Tasklist tasklist)
        {
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
            return View("/Views/Admin/Tasklist/Edit.cshtml", tasklist);
        }

        // GET: Admin/Tasklist/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
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

            return View("/Views/Admin/Tasklist/Delete.cshtml", tasklist);
        }

        // POST: Admin/Tasklist/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tasklist = await _context.Tasklists.FindAsync(id);
            if (tasklist != null)
            {
                _context.Tasklists.Remove(tasklist);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TasklistExists(int id)
        {
            try
            {
                return _context.Tasklists.Any(e => e.ID == id);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return false;
            }
        }
    }
}
