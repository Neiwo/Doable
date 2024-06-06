using Microsoft.AspNetCore.Mvc;
using Doable.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Doable.Data;
using Microsoft.EntityFrameworkCore;

namespace Doable.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var messages = _context.Messages
                .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            foreach (var message in messages)
            {
                Console.WriteLine($"Message ID: {message.MessageId}, Sender ID: {message.SenderId}, Sender Username: {message.Sender?.Username}");
            }

            return View(messages);
        }



        [HttpGet]
        public async Task<IActionResult> SendMessage()
        {
            var users = await _context.Users.ToListAsync();
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int receiverId, string content)
        {
            int? senderId = HttpContext.Session.GetInt32("UserId");
            if (senderId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var message = new Message
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Content = content,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
