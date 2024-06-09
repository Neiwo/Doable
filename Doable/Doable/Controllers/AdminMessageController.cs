using Microsoft.AspNetCore.Mvc;
using Doable.Models;
using Doable.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Doable.Controllers
{
    public class AdminMessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminMessageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(bool showArchived = false)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Message> messagesQuery = _context.Messages
                .Where(m => (m.ReceiverId == adminId || m.SenderId == adminId) && m.ParentMessageId == null);

            if (showArchived)
            {
                messagesQuery = messagesQuery.Where(m => m.Status == "Archived");
            }
            else
            {
                messagesQuery = messagesQuery.Where(m => m.Status != "Archived" && m.Status != "Trash");
            }

            var messages = await messagesQuery
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

            ViewBag.ShowArchived = showArchived; // Pass this to the view

            if (showArchived)
            {
                return View("~/Views/Admin/Message/ArchivedMessages.cshtml", sortedMessages);
            }
            else
            {
                return View("~/Views/Admin/Message/Index.cshtml", sortedMessages);
            }
        }

        public async Task<IActionResult> TrashMessages()
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var messages = await _context.Messages
                .Where(m => (m.ReceiverId == adminId || m.SenderId == adminId) && m.Status == "Trash")
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

            return View("~/Views/Admin/Message/TrashMessages.cshtml", sortedMessages);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = "Active"; // Restore message to "Active" status
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("TrashMessages");
        }


        [HttpGet]
        public async Task<IActionResult> SendMessage()
        {
            var users = await _context.Users.Select(u => new { u.ID, u.Username, u.Role }).ToListAsync();
            ViewBag.Users = users;
            return View("~/Views/Admin/Message/SendMessage.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int receiverId, string content, IFormFile file)
        {
            int? senderId = HttpContext.Session.GetInt32("UserId");
            if (senderId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string filePath = null;
            string fileName = null;

            if (file != null && file.Length > 0)
            {
                fileName = Path.GetFileName(file.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            var message = new Message
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Content = string.IsNullOrWhiteSpace(content) ? null : content, // Handle null content
                Timestamp = DateTime.Now,
                FileName = fileName, // This can be null
                FilePath = filePath // This can be null
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["MessageSent"] = "Message sent!";
            return RedirectToAction("SendMessage");
        }





        public async Task<IActionResult> ViewMessage(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
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

            if (message == null || (message.ReceiverId != adminId && message.SenderId != adminId))
            {
                return NotFound();
            }

            return View("~/Views/Admin/Message/ViewMessage.cshtml", message);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyMessage(int originalMessageId, string content, IFormFile file)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
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

            var receiverId = originalMessage.SenderId == adminId ? originalMessage.ReceiverId : originalMessage.SenderId;

            string filePath = null;
            string fileName = null;

            if (file != null && file.Length > 0)
            {
                fileName = Path.GetFileName(file.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            var replyMessage = new Message
            {
                SenderId = adminId.Value,
                ReceiverId = receiverId,
                Content = string.IsNullOrWhiteSpace(content) ? null : content, // Handle null content
                Timestamp = DateTime.Now,
                ParentMessageId = originalMessageId,
                FileName = fileName, // This can be null
                FilePath = filePath // This can be null
            };

            _context.Messages.Add(replyMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewMessage", new { id = originalMessageId });
        }


        [HttpPost]
        public async Task<IActionResult> ArchiveMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
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
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = "Trash";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }



    }



}
