using BuyZaar.Data;
using BuyZaar.Models;
using BuyZaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public SellerController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.ProductCount = await _context.Products
                .CountAsync(p => p.SellerId == user.Id);

            ViewBag.PendingOrders = await _context.Orders
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Product != null &&
                    oi.Product.SellerId == user.Id &&
                    o.Status == "To Ship"))
                .CountAsync();

            ViewBag.LowStock = await _context.Products
                .CountAsync(p => p.SellerId == user.Id && p.Stock <= 5);

            ViewBag.MonthlySales = await _context.Orders
                .Where(o =>
                    o.OrderItems.Any(oi => oi.Product != null && oi.Product.SellerId == user.Id) &&
                    o.CreatedAt.Month == DateTime.Now.Month &&
                    o.CreatedAt.Year == DateTime.Now.Year &&
                    o.Status == "Completed")
                .SumAsync(o => (decimal?)o.OrderItems
                    .Where(oi => oi.Product != null && oi.Product.SellerId == user.Id)
                    .Sum(oi => oi.Subtotal)) ?? 0;

            return View();
        }

        public async Task<IActionResult> Products()
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.SellerId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

      public async Task<IActionResult> Orders(string? status, string? search)
{
    ViewBag.ActiveRole = "Seller";

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return RedirectToAction("Login", "Account");

    var baseQuery = _context.Orders
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p!.Images)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p!.Seller)
        .Where(o => o.OrderItems.Any(oi =>
            oi.Product != null &&
            oi.Product.SellerId == user.Id))
        .AsQueryable();

    ViewBag.AllCount = await baseQuery.CountAsync();

    ViewBag.ToShipCount = await baseQuery
        .CountAsync(o => o.Status == "To Ship");

    ViewBag.ShippedCount = await baseQuery
        .CountAsync(o => o.Status == "To Receive" || o.Status == "Shipped");

    ViewBag.CompletedCount = await baseQuery
        .CountAsync(o => o.Status == "Completed" || o.Status == "To Review");

    var query = baseQuery;

    if (!string.IsNullOrWhiteSpace(status))
    {
        if (status == "Completed")
        {
            query = query.Where(o =>
                o.Status == "Completed" ||
                o.Status == "To Review");
        }
        else if (status == "To Receive")
        {
            query = query.Where(o =>
                o.Status == "To Receive" ||
                o.Status == "Shipped");
        }
        else
        {
            query = query.Where(o => o.Status == status);
        }
    }

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim();

        query = query.Where(o =>
            o.Id.ToString().Contains(search) ||
            o.ReceiverName.Contains(search) ||
            o.ContactNumber.Contains(search));
    }

    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

    return View(orders);
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShipOrder(int orderId)
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.OrderItems.Any(oi =>
                        oi.Product != null &&
                        oi.Product.SellerId == user.Id));

            if (order == null)
                return NotFound();

            if (order.Status == "To Ship")
            {
                order.Status = "To Receive";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order marked as shipped.";
            }

            return RedirectToAction("Orders");
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            ViewBag.ActiveRole = "Seller";
            return View("create-product");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            ViewBag.ActiveRole = "Seller";

            if (!ModelState.IsValid)
                return View("create-product", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                Category = model.Category,
                AvailableVariants = model.AvailableVariants,
                AvailableSizes = model.AvailableSizes,
                SellerId = user.Id,
                CreatedAt = DateTime.Now
            };

            await AddProductImagesAsync(product, model.ProductImages);

            if (!ModelState.IsValid)
                return View("create-product", model);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added successfully.";
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> ViewProduct(int id)
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category,
                AvailableVariants = product.AvailableVariants,
                AvailableSizes = product.AvailableSizes,
                ExistingImages = product.Images.Select(i => i.ImagePath).ToList()
            };

            return View("edit-product", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            ViewBag.ActiveRole = "Seller";

            if (!ModelState.IsValid)
                return View("edit-product", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.Category = model.Category;
            product.AvailableVariants = model.AvailableVariants;
            product.AvailableSizes = model.AvailableSizes;
            product.UpdatedAt = DateTime.Now;

            await AddProductImagesAsync(product, model.ProductImages);

            if (!ModelState.IsValid)
                return View("edit-product", model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            foreach (var image in product.Images)
            {
                DeleteImageFile(image.ImagePath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> SalesRecords(string? status, string? search, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Images)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Product != null &&
                    oi.Product.SellerId == user.Id))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(o =>
                    o.Id.ToString().Contains(search) ||
                    o.ReceiverName.Contains(search) ||
                    o.ContactNumber.Contains(search));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.TotalSales = orders
                .Where(o => o.Status == "Completed")
                .Sum(o => o.OrderItems
                    .Where(oi => oi.Product != null && oi.Product.SellerId == user.Id)
                    .Sum(oi => oi.Subtotal));

            ViewBag.PendingSales = orders
                .Where(o =>
                    o.Status == "To Ship" ||
                    o.Status == "To Receive" ||
                    o.Status == "To Review" ||
                    o.Status == "Shipped")
                .Sum(o => o.OrderItems
                    .Where(oi => oi.Product != null && oi.Product.SellerId == user.Id)
                    .Sum(oi => oi.Subtotal));

            ViewBag.TotalOrders = orders.Count;

            ViewBag.TotalItemsSold = orders
                .Where(o => o.Status == "Completed")
                .Sum(o => o.OrderItems
                    .Where(oi => oi.Product != null && oi.Product.SellerId == user.Id)
                    .Sum(oi => oi.Quantity));

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductImage(int productId, string imagePath)
        {
            ViewBag.ActiveRole = "Seller";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            var image = product.Images.FirstOrDefault(i => i.ImagePath == imagePath);

            if (image == null)
                return RedirectToAction("EditProduct", new { id = productId });

            DeleteImageFile(image.ImagePath);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product image deleted successfully.";
            return RedirectToAction("EditProduct", new { id = productId });
        }

        private async Task AddProductImagesAsync(Product product, List<IFormFile>? images)
        {
            if (images == null || !images.Any())
                return;

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var image in images)
            {
                if (image.Length <= 0)
                    continue;

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProductImages", "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                    return;
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                product.Images.Add(new ProductImage
                {
                    ImagePath = $"/uploads/products/{fileName}"
                });
            }
        }

        private void DeleteImageFile(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }

        [HttpGet]
public async Task<IActionResult> ShopProfile()
{
    ViewBag.ActiveRole = "Seller";

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return RedirectToAction("Login", "Account");

    var profile = await _context.ShopProfiles
        .FirstOrDefaultAsync(s => s.SellerId == user.Id);

    if (profile == null)
    {
        profile = new ShopProfile
        {
            SellerId = user.Id,
            ShopName = $"{user.UserName}'s Shop",
            CreatedAt = DateTime.Now
        };

        _context.ShopProfiles.Add(profile);
        await _context.SaveChangesAsync();
    }

    return View(profile);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ShopProfile(ShopProfile model, IFormFile? shopLogo)
{
    ViewBag.ActiveRole = "Seller";

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return RedirectToAction("Login", "Account");

    var profile = await _context.ShopProfiles
        .FirstOrDefaultAsync(s => s.SellerId == user.Id);

    if (profile == null)
        return NotFound();

    profile.ShopName = model.ShopName;
profile.ShopDescription = model.ShopDescription;
profile.BusinessEmail = model.BusinessEmail;
profile.BusinessPhone = model.BusinessPhone;
profile.ShopAddress = model.ShopAddress;
profile.City = model.City;
profile.Province = model.Province;
profile.Barangay = model.Barangay;
profile.PostalCode = model.PostalCode;
profile.BusinessHours = model.BusinessHours;
profile.ReturnPolicy = model.ReturnPolicy;
profile.UpdatedAt = DateTime.Now;

    if (shopLogo != null && shopLogo.Length > 0)
    {
        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "shops");

        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(shopLogo.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("LogoPath", "Only JPG, JPEG, PNG, and WEBP files are allowed.");
            return View(profile);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await shopLogo.CopyToAsync(stream);

        profile.LogoPath = $"/uploads/shops/{fileName}";
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Shop profile updated successfully.";
    return RedirectToAction("ShopProfile");
}
    }
}