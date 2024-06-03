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
    [Route("employee/bookings")]
    public class EBookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EBookingController(ApplicationDbContext context)
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

            var bookings = from b in _context.Bookings
                           select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.ID.ToString().Contains(searchString) ||
                    b.CustomerEmail.Contains(searchString) ||
                    b.ServiceType.Contains(searchString) ||
                    b.ServiceDateFrom.ToString().Contains(searchString) ||
                    b.ServiceDateTo.ToString().Contains(searchString) ||
                    b.Status.Contains(searchString) ||
                    b.Payment.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            int totalBookings = await bookings.CountAsync();
            var bookingsOnPage = await bookings.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["Bookings"] = bookingsOnPage;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalBookings / (double)pageSize);

            return View("/Views/Employee/Bookings/Index.cshtml");
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Clients"] = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();

            return View("/Views/Employee/Bookings/Create.cshtml", new Booking());
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Clients"] = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();

            return View("/Views/Employee/Bookings/Create.cshtml", booking);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            ViewData["Clients"] = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();

            return View("/Views/Employee/Bookings/Edit.cshtml", booking);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(Booking booking)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Clients"] = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();

            return View("/Views/Employee/Bookings/Edit.cshtml", booking);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View("/Views/Employee/Bookings/Delete.cshtml", booking);
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
