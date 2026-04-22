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

        public SellerController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var productCount = await _context.Products.CountAsync(p => p.SellerId == user.Id);
            ViewBag.ProductCount = productCount;

            return View();
        }

        public async Task<IActionResult> Products()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var products = await _context.Products
                .Where(p => p.SellerId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
public IActionResult CreateProduct()
{
    return View("create-product"); 
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

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
                SellerId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added successfully.";
            return RedirectToAction("Products");
        }

      [HttpGet]
public async Task<IActionResult> EditProduct(int id)
{
    var product = await _context.Products.FindAsync(id);

    if (product == null)
        return NotFound();

    var model = new ProductViewModel
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Stock = product.Stock,
        Category = product.Category
    };

    return View("edit-product", model); 
}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.Category = model.Category;
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction("Products");
        }
    }
}