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
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            string sellerStatus = "Not Applied";

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
public async Task<IActionResult> BrowseProducts(string? search, string? category, string? sort)
{
    ViewBag.ActiveRole = "Shopper";

    var query = _context.Products
        .Include(p => p.Images)
        .Include(p => p.Seller)
        .Where(p => p.Stock > 0)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(p =>
            p.Name.Contains(search) ||
            p.Description.Contains(search) ||
            p.Category.Contains(search));
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(p => p.Category == category);
    }

    query = sort switch
    {
        "price_low" => query.OrderBy(p => p.Price),
        "price_high" => query.OrderByDescending(p => p.Price),
        "newest" => query.OrderByDescending(p => p.CreatedAt),
        _ => query.OrderByDescending(p => p.CreatedAt)
    };

    var products = await query.ToListAsync();

    ViewBag.Search = search;
    ViewBag.Category = category;
    ViewBag.Sort = sort;

    ViewBag.Categories = await _context.Products
        .Where(p => p.Stock > 0)
        .Select(p => p.Category)
        .Distinct()
        .OrderBy(c => c)
        .ToListAsync();

    return View(products);
}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplySeller(SellerApplicationViewModel model)
        {
            ViewBag.ActiveRole = "Shopper";

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                TempData["ApplicationMessage"] = "Your seller account is already approved.";
                return RedirectToAction("Index");
            }

            var latestApplication = await _context.SellerApplications
                .Where(sa => sa.UserId == user.Id)
                .OrderByDescending(sa => sa.CreatedAt)
                .FirstOrDefaultAsync();

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