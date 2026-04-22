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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            string sellerStatus = "Not Applied";

            // If the user already has the Seller role, always show Approved
            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                sellerStatus = "Approved";
            }
            else
            {
                var latestApplication = await _context.SellerApplications
                    .Where(sa => sa.UserId == user.Id)
                    .OrderByDescending(sa => sa.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestApplication != null)
                {
                    sellerStatus = latestApplication.Status;
                }
            }

            ViewBag.SellerApplicationStatus = sellerStatus;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ApplySeller()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Already a seller: do not allow another application
            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                TempData["ApplicationMessage"] = "Your seller account is already approved.";
                return RedirectToAction("Index");
            }

            var latestApplication = await _context.SellerApplications
                .Where(sa => sa.UserId == user.Id)
                .OrderByDescending(sa => sa.CreatedAt)
                .FirstOrDefaultAsync();

            // If still pending, do not allow re-apply
            if (latestApplication != null && latestApplication.Status == "Pending")
            {
                TempData["ApplicationMessage"] = $"You already have a seller application with status: {latestApplication.Status}.";
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

            // Already a seller: do not allow another application
            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                TempData["ApplicationMessage"] = "Your seller account is already approved.";
                return RedirectToAction("Index");
            }

            var latestApplication = await _context.SellerApplications
                .Where(sa => sa.UserId == user.Id)
                .OrderByDescending(sa => sa.CreatedAt)
                .FirstOrDefaultAsync();

            // If still pending, do not allow re-apply
            if (latestApplication != null && latestApplication.Status == "Pending")
            {
                TempData["ApplicationMessage"] = $"You already submitted a seller application with status: {latestApplication.Status}.";
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