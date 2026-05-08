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
            return View();
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

            if (application.User != null && !await _userManager.IsInRoleAsync(application.User, "Seller"))
                await _userManager.AddToRoleAsync(application.User, "Seller");

            await _context.SaveChangesAsync();

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

            TempData["Message"] = $"Cancellation request for Order #{order.Id} rejected.";
            return RedirectToAction(nameof(CancellationRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReturnedToSeller(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.DeliveryStatus != "Return Pending" &&
                order.DeliveryStatus != "Returned to Seller")
            {
                TempData["Message"] = "This order is not ready to be marked as returned to seller.";
                return RedirectToAction(nameof(Orders));
            }

            order.Status = "Returns";
            order.DeliveryStatus = "Returned to Seller";
            order.RiderId = null;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order #{order.Id} marked as returned to seller.";
            return RedirectToAction(nameof(Orders));
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

        private bool IsRiderNearOrder(string deliveryAddress, string assignedLocation)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress) ||
                string.IsNullOrWhiteSpace(assignedLocation))
                return false;

            var address = NormalizeLocation(deliveryAddress);

            var locationParts = assignedLocation
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => NormalizeLocation(part))
                .Where(part => part.Length >= 3)
                .ToList();

            return locationParts.Any(part => address.Contains(part));
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
                .Trim();
        }
    }
}