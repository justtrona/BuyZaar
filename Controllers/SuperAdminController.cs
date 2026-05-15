using BuyZaar.Data;
using BuyZaar.Models;
using BuyZaar.Services;
using BuyZaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;

        public SuperAdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var shoppers = await _userManager.GetUsersInRoleAsync("Shopper");
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");
            var riders = await _userManager.GetUsersInRoleAsync("Rider");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalShoppers = shoppers.Count;
            ViewBag.TotalSellers = sellers.Count;
            ViewBag.TotalRiders = riders.Count;
            ViewBag.TotalAdmins = admins.Count;

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.VisibleProducts = await _context.Products.CountAsync(p => !p.IsHiddenByAdmin);
            ViewBag.HiddenProducts = await _context.Products.CountAsync(p => p.IsHiddenByAdmin);
            ViewBag.LowStockProducts = await _context.Products.CountAsync(p => p.Stock > 0 && p.Stock <= 5);
            ViewBag.OutOfStockProducts = await _context.Products.CountAsync(p => p.Stock <= 0);

            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            ViewBag.CompletedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Completed" || o.Status == "Delivered");

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Processing");

            ViewBag.CancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == "Cancelled");

            ViewBag.ReturnedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Returns" || o.DeliveryStatus == "Returned to Seller");

            ViewBag.TotalPayments = 0;

            ViewBag.TotalDeliveries = await _context.Orders
                .CountAsync(o => o.DeliveryStatus != null);

            return View();
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View(model);
            }

            var existingUserName = await _userManager.FindByNameAsync(model.UserName);

            if (existingUserName != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return View(model);
            }

            var adminUser = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow
            };

            var temporaryPassword = Guid.NewGuid().ToString("N") + "Aa1!";

            var result = await _userManager.CreateAsync(adminUser, temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            await _userManager.AddToRoleAsync(adminUser, "Admin");

            var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);

            var setupLink = Url.Action(
                "SetupPassword",
                "Account",
                new
                {
                    userId = adminUser.Id,
                    token
                },
                protocol: Request.Scheme
            );

            _emailService.SendEmail(
                adminUser.Email!,
                "Set up your BuyZaar Admin account",
                $@"
                    <h2>Welcome to BuyZaar Admin Team!</h2>
                    <p>Hello {adminUser.FullName},</p>
                    <p>Your admin account has been created by the BuyZaar SuperAdmin.</p>
                    <p>Please click the button/link below to set your password:</p>
                    <p>
                        <a href='{setupLink}'
                           style='display:inline-block;padding:12px 18px;background:#2563eb;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:bold;'>
                            Set My Password
                        </a>
                    </p>
                    <p>If the button does not work, copy and paste this link into your browser:</p>
                    <p>{setupLink}</p>
                "
            );

await LogSuperAdminAuditAsync(
    "Create Admin",                "Admin",
                adminUser.Id,
                $"Created admin account for {adminUser.FullName} ({adminUser.Email}).");

            TempData["AdminCreated"] = "true";
            TempData["CreatedAdminName"] = adminUser.FullName;
            TempData["CreatedAdminEmail"] = adminUser.Email;
            TempData["SuccessMessage"] = "Admin account created successfully. A password setup email has been sent.";

            return RedirectToAction(nameof(CreateAdmin));
        }

        public async Task<IActionResult> Admins(
            string? search,
            string? status,
            string? emailStatus)
        {
            var admins = (await _userManager.GetUsersInRoleAsync("Admin"))
                .OrderBy(a => a.FullName)
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                admins = admins.Where(a =>
                    (!string.IsNullOrWhiteSpace(a.FullName) &&
                     a.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(a.Email) &&
                     a.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(a.UserName) &&
                     a.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(a.PhoneNumber) &&
                     a.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            if (status == "Active")
            {
                admins = admins.Where(a =>
                    a.LockoutEnd == null ||
                    a.LockoutEnd <= DateTimeOffset.Now
                ).ToList();
            }
            else if (status == "Deactivated")
            {
                admins = admins.Where(a =>
                    a.LockoutEnd != null &&
                    a.LockoutEnd > DateTimeOffset.Now
                ).ToList();
            }

            if (emailStatus == "Verified")
            {
                admins = admins.Where(a => a.EmailConfirmed).ToList();
            }
            else if (emailStatus == "Unverified")
            {
                admins = admins.Where(a => !a.EmailConfirmed).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.EmailStatus = emailStatus;
            ViewBag.TotalAdmins = admins.Count;

            return View(admins);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminActivation(string userId)
        {
            var admin = await _userManager.FindByIdAsync(userId);

            if (admin == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(admin, "SuperAdmin"))
            {
                TempData["AdminMessage"] = "SuperAdmin accounts cannot be changed here.";
                return RedirectToAction(nameof(Admins));
            }

            var isDeactivated =
                admin.LockoutEnd != null &&
                admin.LockoutEnd > DateTimeOffset.Now;

            admin.LockoutEnabled = true;
            admin.LockoutEnd = isDeactivated
                ? null
                : DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(admin);

            await LogSuperAdminAuditAsync(
                isDeactivated ? "Activate Admin" : "Deactivate Admin",
                "Admin",
                admin.Id,
                $"{(isDeactivated ? "Activated" : "Deactivated")} admin account {admin.FullName} ({admin.Email}).");

            TempData["AdminMessage"] = isDeactivated
                ? "Admin account activated successfully."
                : "Admin account deactivated successfully.";

            return RedirectToAction(nameof(Admins));
        }

        public async Task<IActionResult> Users(
            string? search,
            string? role,
            string? status,
            string? emailStatus,
            int page = 1)
        {
            const int pageSize = 15;

            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    (!string.IsNullOrWhiteSpace(u.FullName) &&
                     u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.Email) &&
                     u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.UserName) &&
                     u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.PhoneNumber) &&
                     u.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users.Where(u =>
                    userRoles.ContainsKey(u.Id) &&
                    userRoles[u.Id].Contains(role)
                ).ToList();
            }

            if (status == "Active")
            {
                users = users.Where(u =>
                    u.LockoutEnd == null ||
                    u.LockoutEnd <= DateTimeOffset.Now
                ).ToList();
            }
            else if (status == "Deactivated")
            {
                users = users.Where(u =>
                    u.LockoutEnd != null &&
                    u.LockoutEnd > DateTimeOffset.Now
                ).ToList();
            }

            if (emailStatus == "Verified")
            {
                users = users.Where(u => u.EmailConfirmed).ToList();
            }
            else if (emailStatus == "Unverified")
            {
                users = users.Where(u => !u.EmailConfirmed).ToList();
            }

            var totalUsers = users.Count;
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;
            ViewBag.EmailStatus = emailStatus;
            ViewBag.UserRoles = userRoles;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(pagedUsers);
        }

      [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleUserActivation(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);

    if (user == null)
        return NotFound();

    if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
    {
        TempData["UserMessage"] = "SuperAdmin accounts cannot be changed here.";
        return RedirectToAction(nameof(Users));
    }

    var isDeactivated =
        user.LockoutEnd != null &&
        user.LockoutEnd > DateTimeOffset.Now;

    user.LockoutEnabled = true;
    user.LockoutEnd = isDeactivated
        ? null
        : DateTimeOffset.MaxValue;

    await _userManager.UpdateAsync(user);

    await LogSuperAdminAuditAsync(
        isDeactivated ? "Activate User" : "Deactivate User",
        "User",
        user.Id,
        $"{(isDeactivated ? "Activated" : "Deactivated")} user account {user.FullName} ({user.Email}).");

    TempData["UserMessage"] = isDeactivated
        ? "User account activated successfully."
        : "User account deactivated successfully.";

    return RedirectToAction(nameof(Users));
}


        public async Task<IActionResult> Sellers(
            string? search,
            string? status,
            string? emailStatus,
            int page = 1)
        {
            const int pageSize = 15;

            var sellers = (await _userManager.GetUsersInRoleAsync("Seller"))
                .OrderBy(u => u.FullName)
                .ToList();

            var allSellerIds = sellers.Select(s => s.Id).ToList();

            var allSellerShopNames = await _context.ShopProfiles
                .Where(sp => allSellerIds.Contains(sp.SellerId))
                .ToDictionaryAsync(
                    sp => sp.SellerId,
                    sp => sp.ShopName
                );

            if (!string.IsNullOrWhiteSpace(search))
            {
                sellers = sellers.Where(u =>
                    (!string.IsNullOrWhiteSpace(u.FullName) &&
                     u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.Email) &&
                     u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.UserName) &&
                     u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.PhoneNumber) &&
                     u.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (allSellerShopNames.ContainsKey(u.Id) &&
                     !string.IsNullOrWhiteSpace(allSellerShopNames[u.Id]) &&
                     allSellerShopNames[u.Id].Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            if (status == "Active")
            {
                sellers = sellers.Where(u =>
                    u.LockoutEnd == null ||
                    u.LockoutEnd <= DateTimeOffset.Now
                ).ToList();
            }
            else if (status == "Deactivated")
            {
                sellers = sellers.Where(u =>
                    u.LockoutEnd != null &&
                    u.LockoutEnd > DateTimeOffset.Now
                ).ToList();
            }

            if (emailStatus == "Verified")
            {
                sellers = sellers.Where(u => u.EmailConfirmed).ToList();
            }
            else if (emailStatus == "Unverified")
            {
                sellers = sellers.Where(u => !u.EmailConfirmed).ToList();
            }

            var totalSellers = sellers.Count;
            var totalPages = (int)Math.Ceiling(totalSellers / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var pagedSellers = sellers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedSellerIds = pagedSellers.Select(s => s.Id).ToList();

            var sellerShopNames = await _context.ShopProfiles
                .Where(sp => pagedSellerIds.Contains(sp.SellerId))
                .ToDictionaryAsync(
                    sp => sp.SellerId,
                    sp => sp.ShopName
                );

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.EmailStatus = emailStatus;
            ViewBag.TotalSellers = totalSellers;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SellerShopNames = sellerShopNames;

            return View(pagedSellers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSellerActivation(string userId)
        {
            var seller = await _userManager.FindByIdAsync(userId);

            if (seller == null)
                return NotFound();

            var isDeactivated =
                seller.LockoutEnd != null &&
                seller.LockoutEnd > DateTimeOffset.Now;

            seller.LockoutEnabled = true;
            seller.LockoutEnd = isDeactivated
                ? null
                : DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(seller);

            await LogSuperAdminAuditAsync(
                isDeactivated ? "Activate Seller" : "Deactivate Seller",
                "Seller",
                seller.Id,
                $"{(isDeactivated ? "Activated" : "Deactivated")} seller account {seller.FullName} ({seller.Email}).");

            TempData["SellerMessage"] = isDeactivated
                ? "Seller account activated successfully."
                : "Seller account deactivated successfully.";

            return RedirectToAction(nameof(Sellers));
        }

        public async Task<IActionResult> Riders(
            string? search,
            string? status,
            string? vehicle,
            int page = 1)
        {
            const int pageSize = 15;

            var ridersQuery = _context.RiderProfiles
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                ridersQuery = ridersQuery.Where(r =>
                    r.FullName.Contains(search) ||
                    r.PhoneNumber.Contains(search) ||
                    r.AssignedLocation.Contains(search) ||
                    r.VehicleType.Contains(search) ||
                    (r.User != null &&
                     r.User.Email != null &&
                     r.User.Email.Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                ridersQuery = ridersQuery.Where(r => r.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(vehicle))
            {
                ridersQuery = ridersQuery.Where(r => r.VehicleType == vehicle);
            }

            var totalRiders = await ridersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRiders / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var riders = await ridersQuery
                .OrderBy(r => r.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Vehicle = vehicle;
            ViewBag.TotalRiders = totalRiders;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(riders);
        }

        public async Task<IActionResult> Products(
            string? search,
            string? stockStatus,
            string? visibility,
            int page = 1)
        {
            const int pageSize = 15;

            if (page < 1)
                page = 1;

            var productsQuery = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    (p.Seller != null && p.Seller.FullName.Contains(search)) ||
                    (p.Seller != null && p.Seller.Email != null && p.Seller.Email.Contains(search)) ||
                    (p.Seller != null && p.Seller.ShopName != null && p.Seller.ShopName.Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                if (stockStatus == "InStock")
                {
                    productsQuery = productsQuery.Where(p => p.Stock > 5);
                }
                else if (stockStatus == "LowStock")
                {
                    productsQuery = productsQuery.Where(p => p.Stock > 0 && p.Stock <= 5);
                }
                else if (stockStatus == "OutOfStock")
                {
                    productsQuery = productsQuery.Where(p => p.Stock <= 0);
                }
            }

            if (!string.IsNullOrWhiteSpace(visibility))
            {
                if (visibility == "Visible")
                {
                    productsQuery = productsQuery.Where(p => !p.IsHiddenByAdmin);
                }
                else if (visibility == "Hidden")
                {
                    productsQuery = productsQuery.Where(p => p.IsHiddenByAdmin);
                }
            }

            var totalProducts = await productsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.StockStatus = stockStatus;
            ViewBag.Visibility = visibility;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.VisibleProducts = await _context.Products.CountAsync(p => !p.IsHiddenByAdmin);
            ViewBag.HiddenProducts = await _context.Products.CountAsync(p => p.IsHiddenByAdmin);
            ViewBag.LowStockProducts = await _context.Products.CountAsync(p => p.Stock > 0 && p.Stock <= 5);
            ViewBag.OutOfStockProducts = await _context.Products.CountAsync(p => p.Stock <= 0);

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideProduct(int productId, string? reason)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            product.IsHiddenByAdmin = true;
            product.AdminHiddenReason = string.IsNullOrWhiteSpace(reason)
                ? "Product hidden by SuperAdmin."
                : reason.Trim();

            product.HiddenAt = DateTime.Now;

            await _context.SaveChangesAsync();

            

            await LogSuperAdminAuditAsync(
                "Hide Product",
                "Product",
                product.Id.ToString(),
                $"Hidden product '{product.Name}'. Reason: {product.AdminHiddenReason}");

            TempData["ProductMessage"] = "Product hidden successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowProduct(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            product.IsHiddenByAdmin = false;
            product.AdminHiddenReason = null;
            product.HiddenAt = null;

            await _context.SaveChangesAsync();

await LogSuperAdminAuditAsync(                "Show Product",
                "Product",
                product.Id.ToString(),
                $"Restored product '{product.Name}' visibility.");

            TempData["ProductMessage"] = "Product is now visible again.";
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Orders(
            string? search,
            string? orderStatus,
            string? deliveryStatus,
            int page = 1)
        {
            const int pageSize = 15;

            var ordersQuery = _context.Orders
                .Include(o => o.Rider)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Seller)
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Id.ToString().Contains(search) ||
                    o.ReceiverName.Contains(search) ||
                    o.ContactNumber.Contains(search) ||
                    o.DeliveryAddress.Contains(search) ||
                    (o.Rider != null && o.Rider.FullName.Contains(search)) ||
                    o.OrderItems.Any(oi =>
                        oi.Product != null &&
                        (
                            oi.Product.Name.Contains(search) ||
                            (oi.Product.Seller != null &&
                             oi.Product.Seller.FullName.Contains(search))
                        )
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(orderStatus))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == orderStatus);
            }

            if (!string.IsNullOrWhiteSpace(deliveryStatus))
            {
                ordersQuery = ordersQuery.Where(o => o.DeliveryStatus == deliveryStatus);
            }

            var totalOrders = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var orders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.OrderStatus = orderStatus;
            ViewBag.DeliveryStatus = deliveryStatus;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Processing");

            ViewBag.CompletedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Completed" || o.Status == "Delivered");

            ViewBag.CancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == "Cancelled");

            ViewBag.ReturnedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Returns" || o.DeliveryStatus == "Returned to Seller");

            return View(orders);
        }
public async Task<IActionResult> Payments()
{
    ViewBag.PaymentGateway = "PayMongo";
    ViewBag.PaymentStatus = "Configured";

    ViewBag.TotalPayments = await _context.Payments.CountAsync();

    ViewBag.PaidPayments = await _context.Payments
        .CountAsync(p => p.PaymentStatus == "Paid");

    ViewBag.PendingPayments = await _context.Payments
        .CountAsync(p => p.PaymentStatus == "Pending" ||
                         p.PaymentStatus == "Pending Payment");

    ViewBag.FailedPayments = await _context.Payments
        .CountAsync(p => p.PaymentStatus == "Failed");

    ViewBag.TotalPlatformCommission = await _context.PlatformEarnings
        .SumAsync(e => (decimal?)e.CommissionAmount) ?? 0m;

    ViewBag.PendingSettlements = await _context.SellerPayouts
        .CountAsync(p => p.Status == "Pending");

    ViewBag.ReleasedSettlements = await _context.SellerPayouts
        .CountAsync(p => p.Status == "Released");

    return View();
}
        public async Task<IActionResult> Reports()
        {
            var shoppers = await _userManager.GetUsersInRoleAsync("Shopper");
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");
            var riders = await _userManager.GetUsersInRoleAsync("Rider");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalShoppers = shoppers.Count;
            ViewBag.TotalSellers = sellers.Count;
            ViewBag.TotalRiders = riders.Count;
            ViewBag.TotalAdmins = admins.Count;

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.VisibleProducts = await _context.Products.CountAsync(p => !p.IsHiddenByAdmin);
            ViewBag.HiddenProducts = await _context.Products.CountAsync(p => p.IsHiddenByAdmin);
            ViewBag.LowStockProducts = await _context.Products.CountAsync(p => p.Stock > 0 && p.Stock <= 5);
            ViewBag.OutOfStockProducts = await _context.Products.CountAsync(p => p.Stock <= 0);

            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Processing");

            ViewBag.CompletedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Completed" || o.Status == "Delivered");

            ViewBag.CancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == "Cancelled");

            ViewBag.ReturnedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Returns" || o.DeliveryStatus == "Returned to Seller");

            ViewBag.TotalPayments = 0;

            ViewBag.TotalPayments = await _context.Payments.CountAsync();

ViewBag.PaidPayments = await _context.Payments
    .CountAsync(p => p.PaymentStatus == "Paid");

ViewBag.PendingPayments = await _context.Payments
    .CountAsync(p => p.PaymentStatus == "Pending" ||
                     p.PaymentStatus == "Pending Payment");

ViewBag.TotalPlatformCommission = await _context.PlatformEarnings
    .SumAsync(e => (decimal?)e.CommissionAmount) ?? 0m;

ViewBag.PendingSettlements = await _context.SellerPayouts
    .CountAsync(p => p.Status == "Pending");

ViewBag.ReleasedSettlements = await _context.SellerPayouts
    .CountAsync(p => p.Status == "Released");

            return View();
        }

public async Task<IActionResult> AuditLogs(
    string? search,
    string? action,
    string? entityType,
    int page = 1)
{
    const int pageSize = 10;

    var totalLogs = await _context.SuperAdminAuditLogs.CountAsync();

    var totalPages = totalLogs == 0
        ? 1
        : (int)Math.Ceiling(totalLogs / (double)pageSize);

    if (page < 1)
        page = 1;

    if (page > totalPages)
        page = totalPages;

    var logs = await _context.SuperAdminAuditLogs
        .OrderByDescending(a => a.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    ViewBag.Search = search;
    ViewBag.Action = action;
    ViewBag.EntityType = entityType;
    ViewBag.TotalLogs = totalLogs;
    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = totalPages;

    ViewBag.Actions = await _context.SuperAdminAuditLogs
        .Select(a => a.Action)
        .Distinct()
        .OrderBy(a => a)
        .ToListAsync();

    ViewBag.EntityTypes = await _context.SuperAdminAuditLogs
        .Select(a => a.EntityType)
        .Distinct()
        .OrderBy(e => e)
        .ToListAsync();

    return View(logs);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Settings(SystemSetting model)
{
    var settings = await _context.SystemSettings.FirstOrDefaultAsync();

    if (settings == null)
    {
        settings = new SystemSetting();
        _context.SystemSettings.Add(settings);
    }

    settings.AllowShopperRegistration = model.AllowShopperRegistration;
    settings.AllowSellerRegistration = model.AllowSellerRegistration;
    settings.AllowRiderRegistration = model.AllowRiderRegistration;
    settings.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    await LogSuperAdminAuditAsync(
        "Update Settings",
        "System Settings",
        settings.Id.ToString(),
        "Updated registration control settings.");

    TempData["SettingsMessage"] = "Registration controls updated successfully.";

    return RedirectToAction(nameof(Settings));
}

[HttpGet]
public async Task<IActionResult> Settings()
{
    var settings = await _context.SystemSettings.FirstOrDefaultAsync();

    if (settings == null)
    {
        settings = new SystemSetting
        {
            AllowShopperRegistration = true,
            AllowSellerRegistration = true,
            AllowRiderRegistration = true,
            UpdatedAt = DateTime.Now
        };

        _context.SystemSettings.Add(settings);
        await _context.SaveChangesAsync();
    }

    return View(settings);
}
private async Task LogSuperAdminAuditAsync(
    string action,
    string entityType,
    string entityId,
    string description)
{
    var superAdminId = _userManager.GetUserId(User);

    if (string.IsNullOrWhiteSpace(superAdminId))
        return;

    var auditLog = new SuperAdminAuditLog
    {
        SuperAdminId = superAdminId,
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        Description = description,
        CreatedAt = DateTime.Now
    };

    _context.SuperAdminAuditLogs.Add(auditLog);
    await _context.SaveChangesAsync();
}

   public async Task<IActionResult> Payouts(string? search, string? status, int page = 1)
{
    const int pageSize = 15;

    var payoutsQuery = _context.SellerPayouts
        .Include(p => p.Seller)
        .Include(p => p.Order)
        .OrderByDescending(p => p.CreatedAt)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        payoutsQuery = payoutsQuery.Where(p =>
            p.OrderId.ToString().Contains(search) ||
            (p.Seller != null && p.Seller.FullName.Contains(search)) ||
            (p.Seller != null && p.Seller.Email != null && p.Seller.Email.Contains(search))
        );
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        payoutsQuery = payoutsQuery.Where(p => p.Status == status);
    }

    var totalPayouts = await payoutsQuery.CountAsync();
    var totalPages = (int)Math.Ceiling(totalPayouts / (double)pageSize);

    if (page < 1)
        page = 1;

    if (totalPages > 0 && page > totalPages)
        page = totalPages;

    var payouts = await payoutsQuery
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    ViewBag.Search = search;
    ViewBag.Status = status;
    ViewBag.TotalPayouts = totalPayouts;
    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = totalPages;

    ViewBag.PendingPayouts = await _context.SellerPayouts
        .CountAsync(p => p.Status == "Pending");

    ViewBag.ReleasedPayouts = await _context.SellerPayouts
        .CountAsync(p => p.Status == "Released");

    ViewBag.TotalSellerEarnings = await _context.SellerPayouts
        .SumAsync(p => (decimal?)p.SellerEarnings) ?? 0m;

    ViewBag.TotalPlatformCommission = await _context.PlatformEarnings
        .SumAsync(e => (decimal?)e.CommissionAmount) ?? 0m;

    return View(payouts);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ReleasePayout(int payoutId)
{
    var payout = await _context.SellerPayouts
        .Include(p => p.Seller)
        .FirstOrDefaultAsync(p => p.Id == payoutId);

    if (payout == null)
        return NotFound();

    if (payout.Status == "Released")
    {
        TempData["PayoutMessage"] = "This settlement has already been released.";
        return RedirectToAction(nameof(Payouts));
    }

    payout.Status = "Released";
    payout.ReleasedAt = DateTime.Now;

    await _context.SaveChangesAsync();

await LogSuperAdminAuditAsync(        "Release Settlement",
        "Seller Settlement",
        payout.Id.ToString(),
        $"Released settlement for Order #{payout.OrderId}. Seller: {payout.Seller?.Email ?? payout.SellerId}. Net earnings: ₱{payout.SellerEarnings:N2}.");

    TempData["PayoutMessage"] = "Seller settlement marked as released.";

    return RedirectToAction(nameof(Payouts));
}

// [ValidateAntiForgeryToken]
// public async Task<IActionResult> GenerateMissingSettlements()
// {
//     var orders = await _context.Orders
//         .Include(o => o.OrderItems)
//             .ThenInclude(oi => oi.Product)
//         .Where(o =>
//             o.Status == "Delivered" ||
//             o.Status == "Completed" ||
//             o.Status == "To Review")
//         .ToListAsync();

//     foreach (var order in orders)
//     {
//         var alreadyExists = await _context.SellerPayouts
//             .AnyAsync(p => p.OrderId == order.Id);

//         if (alreadyExists)
//             continue;

//         var payment = await _context.Payments
//             .FirstOrDefaultAsync(p => p.OrderId == order.Id);

//         if (payment == null || payment.PaymentStatus != "Paid")
//             continue;

//         var commissionRate = await _context.CommissionRates
//             .Where(c => c.IsActive)
//             .OrderByDescending(c => c.CreatedAt)
//             .FirstOrDefaultAsync();

//         var rate = commissionRate?.RatePercentage ?? 10m;

//         foreach (var item in order.OrderItems)
//         {
//             if (item.Product == null || string.IsNullOrWhiteSpace(item.Product.SellerId))
//                 continue;

//             var productTotal = item.Price * item.Quantity;
//             var commissionAmount = productTotal * (rate / 100m);
//             var sellerEarnings = productTotal - commissionAmount;

//             _context.SellerPayouts.Add(new SellerPayout
//             {
//                 SellerId = item.Product.SellerId,
//                 OrderId = order.Id,
//                 ProductTotal = productTotal,
//                 CommissionAmount = commissionAmount,
//                 SellerEarnings = sellerEarnings,
//                 CommissionRate = rate,
//                 Status = "Pending",
//                 CreatedAt = DateTime.Now
//             });

//             _context.PlatformEarnings.Add(new PlatformEarning
//             {
//                 OrderId = order.Id,
//                 ProductTotal = productTotal,
//                 CommissionAmount = commissionAmount,
//                 CommissionRate = rate,
//                 CreatedAt = DateTime.Now
//             });
//         }
//     }

//     await _context.SaveChangesAsync();

//     TempData["PayoutMessage"] = "Missing seller settlement records generated.";

//     return RedirectToAction(nameof(Payouts));
// }
    }
}