using BuyZaar.Data;
using BuyZaar.Models;
using BuyZaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "Shopper")]
    public class ShopperController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public ShopperController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ApplySeller()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var existingApplication = await _context.SellerApplications
                .FirstOrDefaultAsync(sa => sa.UserId == user.Id);

            if (existingApplication != null)
            {
                TempData["ApplicationMessage"] = $"You already have a seller application with status: {existingApplication.Status}.";
                return RedirectToAction("Index");
            }

            var model = new SellerApplicationViewModel
            {
                FullName = user.FullName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplySeller(SellerApplicationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var existingApplication = await _context.SellerApplications
                .FirstOrDefaultAsync(sa => sa.UserId == user.Id);

            if (existingApplication != null)
            {
                TempData["ApplicationMessage"] = $"You already submitted a seller application with status: {existingApplication.Status}.";
                return RedirectToAction("Index");
            }

            var application = new SellerApplication
            {
                UserId = user.Id,
                FullName = model.FullName,
                ShopName = model.ShopName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                BusinessDescription = model.BusinessDescription,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.SellerApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["ApplicationMessage"] = "Your seller application has been submitted successfully.";
            return RedirectToAction("Index");
        }
    }
}