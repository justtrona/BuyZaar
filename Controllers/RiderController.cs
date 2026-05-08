using BuyZaar.Data;
using BuyZaar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "Rider")]
    public class RiderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RiderController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveRole = "Rider";

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var riderProfile = await _context.RiderProfiles
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == user.Id);

            if (riderProfile == null)
            {
                return NotFound();
            }

            return View(riderProfile);
        }

        public async Task<IActionResult> AssignedOrders()
        {
            ViewBag.ActiveRole = "Rider";

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.RiderId == user.Id && o.DeliveryStatus == "Assigned")
                .OrderByDescending(o => o.AssignedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Assigned")
            {
                TempData["Message"] = "This delivery task cannot be accepted.";
                return RedirectToAction(nameof(AssignedOrders));
            }

            order.DeliveryStatus = "Accepted";
            order.AcceptedAt = DateTime.Now;
            order.Status = "To Receive";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"You accepted delivery for Order #{order.Id}.";
            return RedirectToAction(nameof(ActiveDeliveries));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Assigned")
            {
                TempData["Message"] = "This delivery task cannot be declined.";
                return RedirectToAction(nameof(AssignedOrders));
            }

            order.RiderId = null;
            order.DeliveryStatus = "Pending Assignment";
            order.AssignedAt = null;

            order.Status = "To Receive";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"You declined Order #{order.Id}.";
            return RedirectToAction(nameof(AssignedOrders));
        }

        public async Task<IActionResult> ActiveDeliveries()
        {
            ViewBag.ActiveRole = "Rider";

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var now = DateTime.Now;

            var expiredFailedOrders = await _context.Orders
                .Where(o => o.RiderId == user.Id &&
                            o.DeliveryStatus == "Failed Delivery" &&
                            o.ReturnToSellerAt != null &&
                            o.ReturnToSellerAt <= now)
                .ToListAsync();

            foreach (var order in expiredFailedOrders)
            {
                order.DeliveryStatus = "Returned to Seller";
                order.Status = "Returns";
            }

            if (expiredFailedOrders.Any())
            {
                await _context.SaveChangesAsync();
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.RiderId == user.Id &&
                            (o.DeliveryStatus == "Accepted" ||
                             o.DeliveryStatus == "Picked Up" ||
                             o.DeliveryStatus == "Out for Delivery" ||
                             o.DeliveryStatus == "Failed Delivery"))
                .OrderByDescending(o => o.AcceptedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPickedUp(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Accepted")
            {
                TempData["Message"] = "This order cannot be marked as picked up.";
                return RedirectToAction(nameof(ActiveDeliveries));
            }

            order.DeliveryStatus = "Picked Up";
            order.PickedUpAt = DateTime.Now;
            order.Status = "To Receive";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order #{order.Id} marked as picked up.";
            return RedirectToAction(nameof(ActiveDeliveries));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkOutForDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Picked Up")
            {
                TempData["Message"] = "This order cannot be marked as out for delivery.";
                return RedirectToAction(nameof(ActiveDeliveries));
            }

            order.DeliveryStatus = "Out for Delivery";
            order.Status = "To Receive";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order #{order.Id} is now out for delivery.";
            return RedirectToAction(nameof(ActiveDeliveries));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Out for Delivery")
            {
                TempData["Message"] = "This order cannot be marked as delivered.";
                return RedirectToAction(nameof(ActiveDeliveries));
            }

            order.DeliveryStatus = "Delivered";
            order.Status = "To Receive";
            order.DeliveredAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order #{order.Id} marked as delivered.";
            return RedirectToAction(nameof(ActiveDeliveries));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFailedDelivery(int orderId, string? reason)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Out for Delivery")
            {
                TempData["Message"] = "This order cannot be marked as failed delivery.";
                return RedirectToAction(nameof(ActiveDeliveries));
            }

            order.DeliveryStatus = "Failed Delivery";
            order.Status = "To Receive";
            order.FailedDeliveryAt = DateTime.Now;
            order.FailedDeliveryReason = string.IsNullOrWhiteSpace(reason)
                ? "Failed delivery attempt"
                : reason;

            if (order.ReturnToSellerAt == null)
            {
                order.ReturnToSellerAt = DateTime.Now.AddDays(5);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order #{order.Id} marked as failed delivery. You can retry delivery within 5 days.";
            return RedirectToAction(nameof(ActiveDeliveries));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.RiderId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.DeliveryStatus != "Failed Delivery")
            {
                TempData["Message"] = "Only failed deliveries can be retried.";
                return RedirectToAction(nameof(ActiveDeliveries));
            }

            if (order.ReturnToSellerAt != null && order.ReturnToSellerAt <= DateTime.Now)
            {
                order.DeliveryStatus = "Returned to Seller";
                order.Status = "Returns";

                await _context.SaveChangesAsync();

                TempData["Message"] = $"Order #{order.Id} can no longer be delivered. It has been returned to seller.";
                return RedirectToAction(nameof(ActiveDeliveries));
            }

            order.DeliveryStatus = "Out for Delivery";
            order.Status = "To Receive";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order #{order.Id} is out for delivery again.";
            return RedirectToAction(nameof(ActiveDeliveries));
        }
    }
}