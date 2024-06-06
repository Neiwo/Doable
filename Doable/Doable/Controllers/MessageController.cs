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
                .Where(m => (m.ReceiverId == userId || m.SenderId == userId) && m.ParentMessageId == null)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

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

        public IActionResult ViewMessage(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var message = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Replies)
                    .ThenInclude(r => r.Sender)
                .Include(m => m.Replies)
                    .ThenInclude(r => r.Receiver)
                .FirstOrDefault(m => m.MessageId == id);

            if (message == null || (message.ReceiverId != userId && message.SenderId != userId))
            {
                return NotFound();
            }

            return View(message);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyMessage(int originalMessageId, string content)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var originalMessage = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == originalMessageId);

            if (originalMessage == null)
            {
                return NotFound();
            }

            var receiverId = originalMessage.SenderId == userId ? originalMessage.ReceiverId : originalMessage.SenderId;

            var replyMessage = new Message
            {
                SenderId = userId.Value,
                ReceiverId = receiverId,
                Content = content,
                Timestamp = DateTime.Now,
                ParentMessageId = originalMessageId
            };

            _context.Messages.Add(replyMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewMessage", new { id = originalMessageId });
        }
    }


}
