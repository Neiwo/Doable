using Doable.Services;
using Microsoft.AspNetCore.Mvc;

namespace Doable.Controllers
{
    public class TestController : Controller
    {
        private readonly EmailService _emailService;

        public TestController(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> SendTestEmail()
        {
            await _emailService.SendEmailAsync("doableapp147@gmail.com", "owenyap147@gmail.com", "Test Body");
            return Content("Email sent successfully.");
        }
    }

}
