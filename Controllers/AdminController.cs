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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public AdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

       public async Task<IActionResult> Index()
{
    await AutoMarkReturnPendingOrders();

    ViewBag.TotalUsers = await _context.Users.CountAsync();

    var sellers = await _userManager.GetUsersInRoleAsync("Seller");
    ViewBag.TotalSellers = sellers.Count;

    ViewBag.TotalRiders = await _context.RiderProfiles.CountAsync();

    ViewBag.PendingSellerApplications = await _context.SellerApplications
        .CountAsync(sa => sa.Status == "Pending");

    ViewBag.TotalOrders = await _context.Orders.CountAsync();

    ViewBag.PendingCancellationRequests = await _context.Orders
        .CountAsync(o => o.CancellationRequestStatus == "Pending");

    ViewBag.ReturnPendingOrders = await _context.Orders
        .CountAsync(o => o.DeliveryStatus == "Return Pending");

    ViewBag.SuccessfulReturns = await _context.Orders
        .CountAsync(o => o.DeliveryStatus == "Returned to Seller");

    ViewBag.FailedDeliveries = await _context.Orders
        .CountAsync(o => o.DeliveryStatus == "Failed Delivery");

    var currentMonth = DateTime.Now.Month;
    var currentYear = DateTime.Now.Year;

    ViewBag.TopSellersMonthly = await _context.OrderItems
        .Include(oi => oi.Order)
        .Include(oi => oi.Product)
            .ThenInclude(p => p.Seller)
        .Where(oi =>
            oi.Order != null &&
            oi.Product != null &&
            oi.Product.Seller != null &&
            oi.Order.Status == "Delivered" &&
            oi.Order.DeliveredAt != null &&
            oi.Order.DeliveredAt.Value.Month == currentMonth &&
            oi.Order.DeliveredAt.Value.Year == currentYear)
        .GroupBy(oi => new
        {
            oi.Product.SellerId,
            SellerName = oi.Product.Seller.FullName,
            SellerEmail = oi.Product.Seller.Email
        })
        .Select(g => new
        {
            SellerName = string.IsNullOrWhiteSpace(g.Key.SellerName)
                ? g.Key.SellerEmail
                : g.Key.SellerName,
            TotalSales = g.Sum(oi => oi.Price * oi.Quantity),
            TotalItems = g.Sum(oi => oi.Quantity)
        })
        .OrderByDescending(x => x.TotalSales)
        .Take(10)
        .ToListAsync();

    return View();
}   
        public async Task<IActionResult> Users(string? search, string? role, string? status, int page = 1)
        {
            const int pageSize = 15;

            if (page < 1)
                page = 1;

            var allUsers = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in allUsers)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            var users = allUsers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    (!string.IsNullOrWhiteSpace(u.FullName) &&
                     u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.Email) &&
                     u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.PhoneNumber) &&
                     u.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users.Where(u =>
                    userRoles.ContainsKey(u.Id) &&
                    userRoles[u.Id].Contains(role)
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Active")
                {
                    users = users.Where(u =>
                        u.LockoutEnd == null ||
                        u.LockoutEnd <= DateTimeOffset.UtcNow
                    );
                }
                else if (status == "Deactivated")
                {
                    users = users.Where(u =>
                        u.LockoutEnd != null &&
                        u.LockoutEnd > DateTimeOffset.UtcNow
                    );
                }
            }

            var filteredUsers = users.ToList();

            var totalUsers = filteredUsers.Count;
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var pagedUsers = filteredUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.UserRoles = userRoles;
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;

            return View(pagedUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Message"] = "Admin accounts cannot be deactivated here.";
                return RedirectToAction(nameof(Users));
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(user);

            await LogAdminAction(
                "Deactivate User",
                "User",
                user.Id,
                $"Admin deactivated user account: {user.Email}"
            );

            TempData["Message"] = "User account deactivated successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            await LogAdminAction(
                "Activate User",
                "User",
                user.Id,
                $"Admin reactivated user account: {user.Email}"
            );

            TempData["Message"] = "User account activated successfully.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> SellerApplications()
        {
            var applications = await _context.SellerApplications
                .Include(sa => sa.User)
                .Include(sa => sa.Documents)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        public async Task<IActionResult> ViewSellerApplication(int id)
        {
            var application = await _context.SellerApplications
                .Include(sa => sa.User)
                .Include(sa => sa.Documents)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSeller(int id)
        {
            var application = await _context.SellerApplications
                .Include(sa => sa.User)
                .Include(sa => sa.Documents)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["Message"] = "This application has already been processed.";
                return RedirectToAction(nameof(SellerApplications));
            }

            application.Status = "Approved";

            if (application.User != null &&
                !await _userManager.IsInRoleAsync(application.User, "Seller"))
            {
                await _userManager.AddToRoleAsync(application.User, "Seller");
            }

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Approve Seller Application",
                "SellerApplication",
                application.Id.ToString(),
                $"Admin approved seller application for user: {application.User?.Email}"
            );

            TempData["Message"] = "Seller application approved successfully.";
            return RedirectToAction(nameof(SellerApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSeller(int id)
        {
            var application = await _context.SellerApplications
                .Include(sa => sa.User)
                .Include(sa => sa.Documents)
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

            await LogAdminAction(
                "Reject Seller Application",
                "SellerApplication",
                application.Id.ToString(),
                $"Admin rejected seller application for user: {application.User?.Email}"
            );

            TempData["Message"] = "Seller application rejected.";
            return RedirectToAction(nameof(SellerApplications));
        }

        public async Task<IActionResult> Riders()
        {
            var riders = await _context.RiderProfiles
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(riders);
        }

        [HttpGet]
        public IActionResult CreateRider()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRider(CreateRiderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow
            };

            var temporaryPassword = Guid.NewGuid().ToString("N") + "Aa1!";
            var result = await _userManager.CreateAsync(user, temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Rider");

            var riderProfile = new RiderProfile
            {
                UserId = user.Id,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                AssignedLocation = model.AssignedLocation,
                VehicleType = model.VehicleType,
                Status = "PendingPasswordSetup",
                CreatedAt = DateTime.Now
            };

            _context.RiderProfiles.Add(riderProfile);
            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Create Rider",
                "RiderProfile",
                riderProfile.UserId,
                $"Admin created rider account: {user.Email}"
            );

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var setupLink = Url.Action(
                "SetupPassword",
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme
            );

            _emailService.SendEmail(
                user.Email!,
                "Set up your BuyZaar Rider account",
                $@"
                    <h2>Welcome to BuyZaar Rider Team!</h2>
                    <p>Hello {user.FullName},</p>
                    <p>Your rider account has been created by the BuyZaar admin.</p>
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

            TempData["Message"] = "Rider account created successfully. A password setup email has been sent.";
            return RedirectToAction(nameof(Riders));
        }

        public async Task<IActionResult> Orders()
        {
            await AutoMarkReturnPendingOrders();

            var orders = await _context.Orders
                .Include(o => o.Rider)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var activeRiders = await _context.RiderProfiles
                .Include(r => r.User)
                .Where(r => r.Status == "Active")
                .OrderBy(r => r.FullName)
                .ToListAsync();

            var recommendedRiders = new Dictionary<int, List<RiderProfile>>();

            foreach (var order in orders)
            {
                recommendedRiders[order.Id] = activeRiders
                    .Where(r => IsRiderNearOrder(order.DeliveryAddress, r.AssignedLocation))
                    .ToList();
            }

            ViewBag.RecommendedRiders = recommendedRiders;

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRider(int orderId, string riderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.Status != "Ready for Pickup")
            {
                TempData["Message"] = "This order is not yet ready for rider pickup.";
                return RedirectToAction(nameof(Orders));
            }

            var rider = await _context.RiderProfiles
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == riderId && r.Status == "Active");

            if (rider == null)
            {
                TempData["Message"] = "Selected rider was not found or is not active.";
                return RedirectToAction(nameof(Orders));
            }

            if (!IsRiderNearOrder(order.DeliveryAddress, rider.AssignedLocation))
            {
                TempData["Message"] = "This rider cannot be assigned because they are not near the buyer address.";
                return RedirectToAction(nameof(Orders));
            }

            order.RiderId = rider.UserId;
            order.DeliveryStatus = "Assigned";
            order.AssignedAt = DateTime.Now;
            order.Status = "To Receive";

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Assign Rider",
                "Order",
                order.Id.ToString(),
                $"Admin assigned Order #{order.Id} to rider: {rider.FullName}"
            );

            TempData["Message"] = $"Order #{order.Id} assigned to {rider.FullName}.";
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignRider(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            order.RiderId = null;
            order.DeliveryStatus = "Pending Assignment";
            order.AssignedAt = null;
            order.AcceptedAt = null;
            order.PickedUpAt = null;
            order.DeliveredAt = null;

            if (order.Status == "Assigned to Rider" || order.Status == "To Receive")
                order.Status = "Ready for Pickup";

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Unassign Rider",
                "Order",
                order.Id.ToString(),
                $"Admin removed rider assignment from Order #{order.Id}"
            );

            TempData["Message"] = $"Rider removed from Order #{order.Id}.";
            return RedirectToAction(nameof(Orders));
        }

        public async Task<IActionResult> CancellationRequests()
        {
            await AutoMarkReturnPendingOrders();

            var requests = await _context.Orders
                .Include(o => o.Rider)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CancellationRequestStatus == "Pending")
                .OrderByDescending(o => o.CancellationRequestedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCancellation(int orderId, string? adminNote)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.CancellationRequestStatus != "Pending")
            {
                TempData["Message"] = "This cancellation request has already been processed.";
                return RedirectToAction(nameof(CancellationRequests));
            }

            order.CancellationRequestStatus = "Approved";
            order.CancellationReviewedAt = DateTime.Now;
            order.CancellationAdminNote = string.IsNullOrWhiteSpace(adminNote)
                ? "Cancellation approved by admin."
                : adminNote.Trim();

            order.Status = "Returns";
            order.DeliveryStatus = "Return Pending";
            order.RiderId = null;

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Approve Cancellation",
                "Order",
                order.Id.ToString(),
                $"Admin approved cancellation request for Order #{order.Id}"
            );

            TempData["Message"] = $"Cancellation request for Order #{order.Id} approved.";
            return RedirectToAction(nameof(CancellationRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCancellation(int orderId, string? adminNote)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.CancellationRequestStatus != "Pending")
            {
                TempData["Message"] = "This cancellation request has already been processed.";
                return RedirectToAction(nameof(CancellationRequests));
            }

            order.CancellationRequestStatus = "Rejected";
            order.CancellationReviewedAt = DateTime.Now;
            order.CancellationAdminNote = string.IsNullOrWhiteSpace(adminNote)
                ? "Cancellation rejected by admin."
                : adminNote.Trim();

            order.Status = "To Receive";

            if (order.DeliveryStatus == "Failed Delivery")
                order.DeliveryStatus = "Out for Delivery";

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Reject Cancellation",
                "Order",
                order.Id.ToString(),
                $"Admin rejected cancellation request for Order #{order.Id}"
            );

            TempData["Message"] = $"Cancellation request for Order #{order.Id} rejected.";
            return RedirectToAction(nameof(CancellationRequests));
        }

       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> MarkReturnedToSeller(int orderId)
{
    var order = await _context.Orders
        .FirstOrDefaultAsync(o => o.Id == orderId);

    if (order == null)
        return NotFound();

    if (order.DeliveryStatus != "Return Pending" &&
        order.DeliveryStatus != "Returned to Seller")
    {
        TempData["Message"] = "This order is not ready to be marked as returned to seller.";
        return RedirectToAction(nameof(Orders));
    }

    order.ReturnedByRiderId = order.RiderId;
    order.ReturnedAt = DateTime.Now;

    order.Status = "Returns";
    order.DeliveryStatus = "Returned to Seller";
    order.RiderId = null;

    await _context.SaveChangesAsync();

    await LogAdminAction(
        "Mark Returned To Seller",
        "Order",
        order.Id.ToString(),
        $"Admin marked Order #{order.Id} as returned to seller"
    );

    TempData["Message"] = $"Order #{order.Id} marked as returned to seller.";
    return RedirectToAction(nameof(Orders));
}
        public async Task<IActionResult> Products(string? search, string? stockStatus, string? visibility, int page = 1)
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
                    (p.Seller != null && p.Seller.FullName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                if (stockStatus == "InStock")
                {
                    productsQuery = productsQuery.Where(p => p.Stock > 0);
                }
                else if (stockStatus == "OutOfStock")
                {
                    productsQuery = productsQuery.Where(p => p.Stock <= 0);
                }
                else if (stockStatus == "LowStock")
                {
                    productsQuery = productsQuery.Where(p => p.Stock > 0 && p.Stock <= 5);
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
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideProduct(int productId, string? reason)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound();

            product.IsHiddenByAdmin = true;
            product.AdminHiddenReason = string.IsNullOrWhiteSpace(reason)
                ? "Product hidden by admin."
                : reason.Trim();
            product.HiddenAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Hide Product",
                "Product",
                product.Id.ToString(),
                $"Admin hid product: {product.Name}"
            );

            TempData["Message"] = "Product hidden successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound();

            product.IsHiddenByAdmin = false;
            product.AdminHiddenReason = null;
            product.HiddenAt = null;

            await _context.SaveChangesAsync();

            await LogAdminAction(
                "Show Product",
                "Product",
                product.Id.ToString(),
                $"Admin restored product visibility: {product.Name}"
            );

            TempData["Message"] = "Product is now visible again.";
            return RedirectToAction(nameof(Products));
        }

      public async Task<IActionResult> AuditLogs(
    string? search,
    string? actionFilter,
    string? entityFilter,
    DateTime? fromDate,
    DateTime? toDate,
    int page = 1)
{
    ViewBag.ActiveRole = "Admin";

    const int pageSize = 20;

    if (page < 1)
    {
        page = 1;
    }

    var query = _context.AuditLogs
        .Include(a => a.Admin)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(a =>
            (a.Admin != null && (
                a.Admin.FullName.Contains(search) ||
                a.Admin.Email!.Contains(search)
            )) ||
            a.Action.Contains(search) ||
            a.EntityType.Contains(search) ||
            a.Description.Contains(search));
    }

    if (!string.IsNullOrWhiteSpace(actionFilter))
    {
        query = query.Where(a => a.Action == actionFilter);
    }

    if (!string.IsNullOrWhiteSpace(entityFilter))
    {
        query = query.Where(a => a.EntityType == entityFilter);
    }

    if (fromDate.HasValue)
    {
        query = query.Where(a => a.CreatedAt >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        query = query.Where(a => a.CreatedAt <= toDate.Value.AddDays(1).AddTicks(-1));
    }

    var totalLogs = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

    if (totalPages > 0 && page > totalPages)
    {
        page = totalPages;
    }

    var logs = await query
        .OrderByDescending(a => a.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    ViewBag.Search = search;
    ViewBag.ActionFilter = actionFilter;
    ViewBag.EntityFilter = entityFilter;
    ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
    ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = totalPages;
    ViewBag.TotalLogs = totalLogs;
    ViewBag.PageSize = pageSize;

    ViewBag.ActionOptions = await _context.AuditLogs
        .Select(a => a.Action)
        .Distinct()
        .OrderBy(a => a)
        .ToListAsync();

    ViewBag.EntityOptions = await _context.AuditLogs
        .Select(a => a.EntityType)
        .Distinct()
        .OrderBy(e => e)
        .ToListAsync();

    return View(logs);
}

        public async Task<IActionResult> Reports()
        {
            await AutoMarkReturnPendingOrders();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            ViewBag.TotalOrders = orders.Count;

            ViewBag.TotalSales = orders
                .Where(o => o.Status == "Delivered" || o.DeliveryStatus == "Delivered")
                .Sum(o => o.TotalAmount);

            ViewBag.CompletedOrders = orders
                .Count(o => o.Status == "Delivered" || o.DeliveryStatus == "Delivered");

            ViewBag.PendingOrders = orders
                .Count(o => o.Status == "Pending Payment" || o.Status == "Ready for Pickup");

            ViewBag.CancelledOrders = orders
                .Count(o => o.CancellationRequestStatus == "Approved");

            ViewBag.ReturnedOrders = orders
                .Count(o => o.DeliveryStatus == "Returned to Seller");

            ViewBag.FailedDeliveries = orders
                .Count(o => o.DeliveryStatus == "Failed Delivery");

            ViewBag.TotalUsers = await _context.Users.CountAsync();

            var shoppers = await _userManager.GetUsersInRoleAsync("Shopper");
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");
            var riders = await _userManager.GetUsersInRoleAsync("Rider");

            ViewBag.TotalShoppers = shoppers.Count;
            ViewBag.TotalSellers = sellers.Count;
            ViewBag.TotalRiders = riders.Count;

            ViewBag.PendingSellerApplications = await _context.SellerApplications
                .CountAsync(sa => sa.Status == "Pending");

            ViewBag.TotalProducts = await _context.Products.CountAsync();

            ViewBag.HiddenProducts = await _context.Products
                .CountAsync(p => p.IsHiddenByAdmin);

            ViewBag.LowStockProducts = await _context.Products
                .CountAsync(p => p.Stock > 0 && p.Stock <= 10);

            ViewBag.OutOfStockProducts = await _context.Products
                .CountAsync(p => p.Stock <= 0);

            var topProducts = orders
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.Product != null)
                .GroupBy(oi => oi.Product!.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalSales = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToList();

            ViewBag.TopProducts = topProducts;

            var recentOrders = orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        private async Task AutoMarkReturnPendingOrders()
        {
            var now = DateTime.Now;

            var expiredOrders = await _context.Orders
                .Where(o =>
                    o.DeliveryStatus == "Failed Delivery" &&
                    o.ReturnToSellerAt != null &&
                    o.ReturnToSellerAt <= now)
                .ToListAsync();

            if (!expiredOrders.Any())
                return;

            foreach (var order in expiredOrders)
            {
                order.Status = "Returns";
                order.DeliveryStatus = "Return Pending";
                order.RiderId = null;
            }

            await _context.SaveChangesAsync();
        }

        private async Task LogAdminAction(
            string action,
            string entityType,
            string entityId,
            string description)
        {
            var adminId = _userManager.GetUserId(User);

            var log = new AuditLog
            {
                AdminId = adminId ?? string.Empty,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private bool IsRiderNearOrder(string deliveryAddress, string assignedLocation)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress) ||
                string.IsNullOrWhiteSpace(assignedLocation))
            {
                return false;
            }

            var normalizedAddress = NormalizeLocation(deliveryAddress);

            var assignedLocations = assignedLocation
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(location => NormalizeLocation(location))
                .Where(location => location.Length >= 3)
                .ToList();

            return assignedLocations.Any(location =>
                normalizedAddress.Contains(location) ||
                location.Contains(normalizedAddress));
        }

        private string NormalizeLocation(string value)
        {
            return value
                .ToLower()
                .Replace(".", "")
                .Replace("-", " ")
                .Replace("city of", "")
                .Replace("city", "")
                .Replace("municipality of", "")
                .Replace("municipality", "")
                .Replace("barangay", "")
                .Replace("brgy", "")
                .Trim();
        }

public async Task<IActionResult> Payments(string? search, string? status, string? method, int page = 1)
{
    const int pageSize = 15;

    if (page < 1)
        page = 1;

    var paymentsQuery = _context.Payments
        .Include(p => p.Order)
            .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Seller)
        .OrderByDescending(p => p.CreatedAt)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        paymentsQuery = paymentsQuery.Where(p =>
            p.Id.ToString().Contains(search) ||
            p.OrderId.ToString().Contains(search) ||
            (p.ReferenceNumber != null && p.ReferenceNumber.Contains(search)) ||
            (p.Order != null && p.Order.ReceiverName.Contains(search)) ||
            (p.Order != null && p.Order.ContactNumber.Contains(search)) ||
            (p.Order != null && p.Order.OrderItems.Any(oi =>
                oi.Product != null &&
                (
                    oi.Product.Name.Contains(search) ||
                    oi.Product.Seller.FullName.Contains(search) ||
                    oi.Product.Seller.Email!.Contains(search)
                )))
        );
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        paymentsQuery = paymentsQuery.Where(p => p.PaymentStatus == status);
    }

    if (!string.IsNullOrWhiteSpace(method))
    {
        paymentsQuery = paymentsQuery.Where(p => p.PaymentMethod == method);
    }

    var totalPayments = await paymentsQuery.CountAsync();
    var totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);

    if (totalPages < 1)
        totalPages = 1;

    if (page > totalPages)
        page = totalPages;

    var payments = await paymentsQuery
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    ViewBag.Search = search;
    ViewBag.Status = status;
    ViewBag.Method = method;
    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = totalPages;
    ViewBag.TotalPayments = totalPayments;

    ViewBag.TotalRevenue = await _context.Payments
        .Where(p => p.PaymentStatus == "Paid")
        .SumAsync(p => p.Amount);

    ViewBag.TotalRefunded = await _context.Payments
        .Where(p => p.IsRefunded)
        .SumAsync(p => p.RefundAmount ?? 0);

    ViewBag.ReturnedPayments = await _context.Payments
        .Include(p => p.Order)
        .CountAsync(p =>
            p.Order != null &&
            (
                p.Order.Status == "Returns" ||
                p.Order.DeliveryStatus == "Returned to Seller"
            ));

    ViewBag.PendingPayments = await _context.Payments
        .CountAsync(p => p.PaymentStatus == "Pending");

    ViewBag.PaidPayments = await _context.Payments
        .CountAsync(p => p.PaymentStatus == "Paid");

    return View(payments);
}

public async Task<IActionResult> ManageSeller(
    string? search,
    string? verificationStatus,
    string? emailStatus,
    string? accountStatus)
{
    ViewBag.ActiveRole = "Admin";

    var sellers = (await _userManager.GetUsersInRoleAsync("Seller")).ToList();

    if (!string.IsNullOrWhiteSpace(search))
    {
        sellers = sellers.Where(s =>
            (!string.IsNullOrWhiteSpace(s.FullName) &&
             s.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(s.Email) &&
             s.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    if (verificationStatus == "Verified")
    {
        sellers = sellers.Where(s => s.IsVerified).ToList();
    }
    else if (verificationStatus == "Pending")
    {
        sellers = sellers.Where(s => !s.IsVerified).ToList();
    }

    if (emailStatus == "Verified")
    {
        sellers = sellers.Where(s => s.EmailConfirmed).ToList();
    }
    else if (emailStatus == "Unverified")
    {
        sellers = sellers.Where(s => !s.EmailConfirmed).ToList();
    }

    if (accountStatus == "Active")
    {
        sellers = sellers.Where(s =>
            s.LockoutEnd == null ||
            s.LockoutEnd <= DateTimeOffset.Now
        ).ToList();
    }
    else if (accountStatus == "Deactivated")
    {
        sellers = sellers.Where(s =>
            s.LockoutEnd != null &&
            s.LockoutEnd > DateTimeOffset.Now
        ).ToList();
    }

    var sellerIds = sellers.Select(s => s.Id).ToList();

    var sellerSales = await _context.OrderItems
        .Include(oi => oi.Order)
        .Include(oi => oi.Product)
        .Where(oi =>
            oi.Order != null &&
            oi.Product != null &&
            sellerIds.Contains(oi.Product.SellerId) &&
            (
                oi.Order.Status == "Delivered" ||
                oi.Order.DeliveryStatus == "Delivered"
            ))
        .GroupBy(oi => oi.Product.SellerId)
        .Select(g => new
        {
            SellerId = g.Key,
            TotalSales = g.Sum(oi => oi.Price * oi.Quantity)
        })
        .ToDictionaryAsync(x => x.SellerId, x => x.TotalSales);

    ViewBag.Search = search;
    ViewBag.VerificationStatus = verificationStatus;
    ViewBag.EmailStatus = emailStatus;
    ViewBag.AccountStatus = accountStatus;
    ViewBag.SellerSales = sellerSales;

    return View(sellers);
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleSellerActivation(string userId)
{
    var seller = await _userManager.FindByIdAsync(userId);

    if (seller == null)
    {
        return NotFound();
    }

    var isDeactivated = seller.LockoutEnd != null && seller.LockoutEnd > DateTimeOffset.Now;

    seller.LockoutEnd = isDeactivated
        ? null
        : DateTimeOffset.MaxValue;

    await _userManager.UpdateAsync(seller);

    TempData["SellerMessage"] = isDeactivated
        ? "Seller account activated successfully."
        : "Seller account deactivated successfully.";

    return RedirectToAction(nameof(ManageSeller));
}

public async Task<IActionResult> ReturnedOrders()
{
    ViewBag.ActiveRole = "Admin";

    var returnedOrders = await _context.Orders
        .Include(o => o.Rider)
        .Include(o => o.ReturnedByRider)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Seller)
        .Where(o =>
            o.Status == "Returns" ||
            o.DeliveryStatus == "Return Pending" ||
            o.DeliveryStatus == "Returned to Seller")
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

    return View(returnedOrders);
}

public async Task<IActionResult> CancelledOrders()
{
    ViewBag.ActiveRole = "Admin";

    var cancelledOrders = await _context.Orders
        .Include(o => o.Rider)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Seller)
        .Where(o =>
            o.CancellationRequestStatus == "Approved" &&
            (
                o.RiderId == null ||
                o.DeliveryStatus == "Pending Assignment" ||
                o.DeliveryStatus == "Assigned" ||
                o.DeliveryStatus == "Return Pending"
            ))
        .OrderByDescending(o => o.CancellationReviewedAt ?? o.CreatedAt)
        .ToListAsync();

    return View(cancelledOrders);
}

// [HttpPost]
// [ValidateAntiForgeryToken]
// public async Task<IActionResult> ToggleSellerVerification(string userId)
// {
//     var seller = await _userManager.FindByIdAsync(userId);

//     if (seller == null)
//     {
//         return NotFound();
//     }

//     seller.IsVerified = !seller.IsVerified;
//     seller.VerifiedAt = seller.IsVerified ? DateTime.Now : null;

//     await _userManager.UpdateAsync(seller);

//     TempData["SellerMessage"] = seller.IsVerified
//         ? "Seller verified successfully."
//         : "Seller verification removed.";

//     return RedirectToAction(nameof(ManageSeller));
// }
    }
}