using Microsoft.AspNetCore.Mvc;
using BuyZaar.Services;

namespace BuyZaar.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailService _emailService;

        // Inject EmailService
        public HomeController(EmailService emailService)
        {
            _emailService = emailService;
        }

        //  Landing Page as the Index
        public IActionResult Index()
        {
return View();     }

        public IActionResult Privacy()
        {
            return View();
        }

        // Test Email Action (for testing email functionality)
        public IActionResult TestEmail()
        {
            try
            {
                _emailService.SendEmail(
                    "iujieungaming@gmail.com", // 🔁 change this to your real email
                    "Buyzaar Test Email",
                    "<h2>SendGrid is working!</h2><p>Your SMTP setup is successful.</p>"
                );

                return Content("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
    }
}