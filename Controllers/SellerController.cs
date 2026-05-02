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
    }
}