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

            var latestApplication = await _context.SellerApplications
                .Include(sa => sa.Documents)
                .Where(sa => sa.UserId == user.Id)
                .OrderByDescending(sa => sa.CreatedAt)
                .FirstOrDefaultAsync();

            if (await _userManager.IsInRoleAsync(user, "Seller"))
                sellerStatus = "Approved";
            else if (latestApplication != null)
                sellerStatus = latestApplication.Status;

            ViewBag.SellerApplicationStatus = sellerStatus;
            ViewBag.LatestSellerApplication = latestApplication;

            ViewBag.TotalOrders = await _context.Orders.CountAsync(o => o.ShopperId == user.Id);

            ViewBag.CartItems = await _context.CartItems
                .Where(c => c.ShopperId == user.Id)
                .SumAsync(c => c.Quantity);

            ViewBag.PendingDeliveries = await _context.Orders
                .CountAsync(o => o.ShopperId == user.Id && o.Status == "To Receive");

            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.ShopperId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Take(3)
                .ToListAsync();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string phoneNumber)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            user.FullName = string.IsNullOrWhiteSpace(fullName) ? user.FullName : fullName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

            var result = await _userManager.UpdateAsync(user);

            TempData["ProfileMessage"] = result.Succeeded
                ? "Profile updated successfully."
                : "Unable to update profile. Please try again.";

            return RedirectToAction("Index");
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
                query = query.Where(p => p.Category == category);

            query = sort switch
            {
                "price_low" => query.OrderBy(p => p.Price),
                "price_high" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Sort = sort;

            ViewBag.Categories = await _context.Products
                .Where(p => p.Stock > 0)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(await query.ToListAsync());
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

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.Stock > 0);
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
                _context.CartItems.Add(new CartItem
                {
                    ShopperId = user.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    SelectedVariant = selectedVariant,
                    SelectedSize = selectedSize,
                    CreatedAt = DateTime.Now
                });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNowCheckout(int productId, int quantity, string? selectedVariant, string? selectedSize)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (quantity < 1)
                quantity = 1;

            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == productId && p.Stock > 0);

            if (product == null)
                return NotFound();

            if (quantity > product.Stock)
            {
                TempData["ErrorMessage"] = "Not enough stock available.";
                return RedirectToAction("ViewDetails", new { id = productId });
            }

            var subtotal = product.Price * quantity;
            var shippingFee = subtotal >= 2500m ? 0m : 60m;

            var model = new CheckoutViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImage = product.Images.FirstOrDefault()?.ImagePath ?? "/images/no-image.png",
                Price = product.Price,
                Quantity = quantity,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                SelectedVariant = string.IsNullOrWhiteSpace(selectedVariant) ? null : selectedVariant.Trim(),
                SelectedSize = string.IsNullOrWhiteSpace(selectedSize) ? null : selectedSize.Trim(),
                ReceiverName = user.FullName ?? user.UserName ?? "",
                ContactNumber = user.PhoneNumber ?? "",
                DeliveryAddress = ""
            };

            return View("Checkout", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutSelectedCart(List<int> selectedCartItemIds)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (selectedCartItemIds == null || !selectedCartItemIds.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one item to checkout.";
                return RedirectToAction("MyCart");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Images)
                .Where(c => selectedCartItemIds.Contains(c.Id) && c.ShopperId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("MyCart");

            var firstItem = cartItems.First();
            var product = firstItem.Product!;
            var subtotal = cartItems.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);
            var shippingFee = subtotal >= 2500m ? 0m : 60m;

            var model = new CheckoutViewModel
            {
                ProductId = product.Id,
                ProductName = cartItems.Count == 1 ? product.Name : $"{cartItems.Count} selected items",
                ProductImage = product.Images.FirstOrDefault()?.ImagePath ?? "/images/no-image.png",
                Price = product.Price,
                Quantity = firstItem.Quantity,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                SelectedVariant = firstItem.SelectedVariant,
                SelectedSize = firstItem.SelectedSize,
                ReceiverName = user.FullName ?? user.UserName ?? "",
                ContactNumber = user.PhoneNumber ?? "",
                DeliveryAddress = ""
            };

            return View("Checkout", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.ReceiverName) ||
                string.IsNullOrWhiteSpace(model.ContactNumber) ||
                string.IsNullOrWhiteSpace(model.HouseNo) ||
                string.IsNullOrWhiteSpace(model.Street) ||
                string.IsNullOrWhiteSpace(model.Barangay) ||
                string.IsNullOrWhiteSpace(model.CityMunicipality) ||
                string.IsNullOrWhiteSpace(model.Province))
            {
                TempData["ErrorMessage"] = "Please complete receiver name, contact number, and delivery address.";
                return View("Checkout", model);
            }

            model.DeliveryAddress =
                $"{model.HouseNo.Trim()}, {model.Street.Trim()}, {model.Barangay.Trim()}, {model.CityMunicipality.Trim()}, {model.Province.Trim()}" +
                (string.IsNullOrWhiteSpace(model.Landmark) ? "" : $", Landmark: {model.Landmark.Trim()}");

            if (model.Quantity < 1)
                model.Quantity = 1;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == model.ProductId);

                if (product == null)
                    return NotFound();

                if (product.Stock < model.Quantity)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Not enough stock available.";
                    return RedirectToAction("ViewDetails", new { id = model.ProductId });
                }

                var subtotal = product.Price * model.Quantity;
                var shippingFee = subtotal >= 2500m ? 0m : 60m;

                var order = new Order
                {
                    ShopperId = user.Id,
                    ReceiverName = model.ReceiverName.Trim(),
                    ContactNumber = model.ContactNumber.Trim(),
                    DeliveryAddress = model.DeliveryAddress.Trim(),
                    ShippingFee = shippingFee,
                    TotalAmount = subtotal + shippingFee,
                    Status = "To Ship",
                    CreatedAt = DateTime.Now
                };

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = model.Quantity,
                    Price = product.Price,
                    Subtotal = subtotal,
                    SelectedVariant = string.IsNullOrWhiteSpace(model.SelectedVariant) ? null : model.SelectedVariant.Trim(),
                    SelectedSize = string.IsNullOrWhiteSpace(model.SelectedSize) ? null : model.SelectedSize.Trim()
                });

                product.Stock -= model.Quantity;

                _context.Orders.Add(order);
                _context.Products.Update(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Order created successfully.";
                return RedirectToAction("OrderSuccess", new { id = order.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Something went wrong while placing your order.";
                return View("Checkout", model);
            }
        }

        public async Task<IActionResult> OrderSuccess(int id)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(o => o.Id == id && o.ShopperId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public async Task<IActionResult> MyOrders(string? status)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Images)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Seller)
                .Where(o => o.ShopperId == user.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "To Pay")
                    query = query.Where(o => o.Status == "To Pay" || o.Status == "Pending Payment");
                else
                    query = query.Where(o => o.Status == status);
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Images)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Seller)
                .FirstOrDefaultAsync(o => o.Id == id && o.ShopperId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkOrderReceived(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ShopperId == user.Id);

            if (order == null)
                return NotFound();

            order.Status = "To Review";
            await _context.SaveChangesAsync();

            return RedirectToAction("MyOrders", new { status = "To Review" });
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

            if (latestApplication != null &&
                latestApplication.Status != "Cancelled" &&
                latestApplication.Status != "Rejected")
            {
                return RedirectToAction("SellerApplicationDetails", new { id = latestApplication.Id });
            }

            return View(new SellerApplicationViewModel
            {
                FullName = user.FullName ?? ""
            });
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

            if (latestApplication != null &&
                latestApplication.Status != "Cancelled" &&
                latestApplication.Status != "Rejected")
            {
                return RedirectToAction("SellerApplicationDetails", new { id = latestApplication.Id });
            }

            var application = new SellerApplication
            {
                UserId = user.Id,
                FullName = model.FullName.Trim(),
                ShopName = model.ShopName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Address = model.Address.Trim(),
                BusinessDescription = model.BusinessDescription.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            if (model.Documents != null && model.Documents.Any())
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };

                foreach (var file in model.Documents)
                {
                    if (file.Length <= 0)
                        continue;

                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Documents", "Only PDF, DOC, DOCX, PNG, JPG, and JPEG files are allowed.");
                        return View(model);
                    }

                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);

                    application.Documents.Add(new SellerApplicationDocument
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        FileData = memoryStream.ToArray(),
                        UploadedAt = DateTime.Now
                    });
                }
            }

            _context.SellerApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["ApplicationMessage"] = "Your seller application has been submitted successfully.";
            return RedirectToAction("SellerApplicationDetails", new { id = application.Id });
        }

        public async Task<IActionResult> SellerApplicationDetails(int id)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var application = await _context.SellerApplications
                .Include(sa => sa.Documents)
                .FirstOrDefaultAsync(sa => sa.Id == id && sa.UserId == user.Id);

            if (application == null)
                return NotFound();

            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSellerApplication(int applicationId)
        {
            ViewBag.ActiveRole = "Shopper";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var application = await _context.SellerApplications
                .FirstOrDefaultAsync(sa => sa.Id == applicationId && sa.UserId == user.Id);

            if (application == null)
                return NotFound();

            if (application.Status == "Approved")
            {
                TempData["ApplicationMessage"] = "Approved applications cannot be cancelled.";
                return RedirectToAction("Index");
            }

            application.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["ApplicationMessage"] = "Your seller application has been cancelled.";
            return RedirectToAction("Index");
        }
    }
}