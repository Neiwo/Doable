using Microsoft.AspNetCore.Mvc;
using Doable.Models;
using Doable.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace Doable.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var messages = await _context.Messages
                .Where(m => (m.ReceiverId == userId || m.SenderId == userId) && m.ParentMessageId == null && m.Status != "Archived")
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Replies)
                .ToListAsync();

            var sortedMessages = messages
                .Select(m => new
                {
                    Message = m,
                    LatestReply = m.Replies.OrderByDescending(r => r.Timestamp).FirstOrDefault(),
                    SenderRole = m.Sender?.Role,
                    ReceiverRole = m.Receiver?.Role
                })
                .OrderByDescending(m => m.LatestReply?.Timestamp ?? m.Message.Timestamp)
                .ToList();

            return View("~/Views/Employee/Message/Index.cshtml", sortedMessages);
        }

        [HttpGet]
        public async Task<IActionResult> SendMessage()
        {
            var users = await _context.Users.ToListAsync();
            ViewBag.Users = users;
            return View("~/Views/Employee/Message/SendMessage.cshtml");
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

        public async Task<IActionResult> ViewMessage(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Replies)
                    .ThenInclude(r => r.Sender)
                .Include(m => m.Replies)
                    .ThenInclude(r => r.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null || (message.ReceiverId != userId && message.SenderId != userId))
            {
                return NotFound();
            }

            return View("~/Views/Employee/Message/ViewMessage.cshtml", message);
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

        [HttpPost]
        public async Task<IActionResult> ArchiveMessage(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null || (message.ReceiverId != userId && message.SenderId != userId))
            {
                return NotFound();
            }

            message.Status = "Archived";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null || (message.ReceiverId != userId && message.SenderId != userId))
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
