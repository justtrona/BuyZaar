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
                    sellerStatus = latestApplication.Status;
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

        public async Task<IActionResult> ViewDetails(int id)
        {
            ViewBag.ActiveRole = "Shopper";

            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id && p.Stock > 0);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity, string? selectedVariant, string? selectedSize)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (quantity < 1)
                quantity = 1;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.Stock > 0);

            if (product == null)
                return NotFound();

            if (quantity > product.Stock)
                quantity = product.Stock;

            selectedVariant = string.IsNullOrWhiteSpace(selectedVariant) ? null : selectedVariant.Trim();
            selectedSize = string.IsNullOrWhiteSpace(selectedSize) ? null : selectedSize.Trim();

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.ShopperId == user.Id &&
                    c.ProductId == productId &&
                    c.SelectedVariant == selectedVariant &&
                    c.SelectedSize == selectedSize);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;

                if (existingCartItem.Quantity > product.Stock)
                    existingCartItem.Quantity = product.Stock;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ShopperId = user.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    SelectedVariant = selectedVariant,
                    SelectedSize = selectedSize,
                    CreatedAt = DateTime.Now
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added to cart.";
            return RedirectToAction("MyCart");
        }

        public async Task<IActionResult> MyCart()
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Images)
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Seller)
                .Where(c => c.ShopperId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartQuantity(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.ShopperId == user.Id);

            if (cartItem == null)
                return NotFound();

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                var maxStock = cartItem.Product?.Stock ?? quantity;
                cartItem.Quantity = quantity > maxStock ? maxStock : quantity;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartQuantityAjax([FromBody] UpdateCartQtyVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == model.Id && c.ShopperId == user.Id);

            if (cartItem == null)
                return Json(new { success = false, message = "Cart item not found." });

            if (model.Quantity < 1)
                model.Quantity = 1;

            var maxStock = cartItem.Product?.Stock ?? model.Quantity;
            cartItem.Quantity = model.Quantity > maxStock ? maxStock : model.Quantity;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                quantity = cartItem.Quantity
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.ShopperId == user.Id);

            if (cartItem == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyCart");
        }

        [HttpGet]
        public async Task<IActionResult> ApplySeller()
        {
            ViewBag.ActiveRole = "Shopper";

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