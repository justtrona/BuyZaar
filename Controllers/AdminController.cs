using BuyZaar.Data;
using BuyZaar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SellerApplications()
        {
            var applications = await _context.SellerApplications
                .Include(sa => sa.User)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSeller(int id)
        {
            var application = await _context.SellerApplications
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["Message"] = "This application has already been processed.";
                return RedirectToAction(nameof(SellerApplications));
            }

            application.Status = "Approved";

            if (application.User != null)
            {
                var user = application.User;

                if (!await _userManager.IsInRoleAsync(user, "Seller"))
                {
                    await _userManager.AddToRoleAsync(user, "Seller");
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Seller application approved successfully.";
            return RedirectToAction(nameof(SellerApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSeller(int id)
        {
            var application = await _context.SellerApplications
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["Message"] = "This application has already been processed.";
                return RedirectToAction(nameof(SellerApplications));
            }

            application.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Message"] = "Seller application rejected.";
            return RedirectToAction(nameof(SellerApplications));
        }
    }
}