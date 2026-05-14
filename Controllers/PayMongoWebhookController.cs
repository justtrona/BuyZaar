using System.Text.Json;
using BuyZaar.Data;
using BuyZaar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    [ApiController]
    [Route("api/paymongo/webhook")]
    public class PayMongoWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PayMongoWebhookController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return BadRequest();

            string? eventType = null;
            int? parsedOrderId = null;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                eventType = root
                    .GetProperty("data")
                    .GetProperty("attributes")
                    .GetProperty("type")
                    .GetString();

                if (eventType != "checkout_session.payment.paid")
                {
                    await SaveWebhookLogAsync(
                        eventType,
                        null,
                        "Ignored",
                        body,
                        "Webhook received but event type was ignored."
                    );

                    return Ok(new
                    {
                        received = true,
                        ignored = true,
                        eventType
                    });
                }

                var attributes = root
                    .GetProperty("data")
                    .GetProperty("attributes");

                var checkoutSession = attributes
                    .GetProperty("data")
                    .GetProperty("attributes");

                var referenceNumber = checkoutSession
                    .GetProperty("reference_number")
                    .GetString();

                if (string.IsNullOrWhiteSpace(referenceNumber))
                {
                    await SaveWebhookLogAsync(
                        eventType,
                        null,
                        "Failed",
                        body,
                        "Missing reference number."
                    );

                    return Ok();
                }

                var orderIdText = referenceNumber.Replace("ORDER-", "");

                if (!int.TryParse(orderIdText, out var orderId))
                {
                    await SaveWebhookLogAsync(
                        eventType,
                        null,
                        "Failed",
                        body,
                        "Invalid order reference number."
                    );

                    return Ok();
                }

                parsedOrderId = orderId;

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    await SaveWebhookLogAsync(
                        eventType,
                        parsedOrderId,
                        "Failed",
                        body,
                        "Order not found."
                    );

                    return Ok();
                }

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == order.Id);

                if (payment != null)
                {
                    payment.PaymentStatus = "Paid";
                    payment.PaidAt = DateTime.Now;
                }

                order.Status = "To Ship";

                await _context.SaveChangesAsync();

                await CreateMarketplaceFinancialRecordsAsync(order.Id);

                await SaveWebhookLogAsync(
                    eventType,
                    order.Id,
                    "Processed",
                    body,
                    "Payment marked as paid and settlement records generated."
                );

                return Ok(new
                {
                    received = true,
                    orderId = order.Id,
                    status = "Paid"
                });
            }
            catch (Exception ex)
            {
                await SaveWebhookLogAsync(
                    eventType,
                    parsedOrderId,
                    "Error",
                    body,
                    ex.Message
                );

                return Ok(new
                {
                    received = true,
                    error = true
                });
            }
        }

        private async Task SaveWebhookLogAsync(
            string? eventType,
            int? orderId,
            string status,
            string payload,
            string message)
        {
            _context.PayMongoWebhookLogs.Add(new PayMongoWebhookLog
            {
                EventType = eventType,
                OrderId = orderId,
                Status = status,
                Payload = payload,
                Message = message,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private async Task CreateMarketplaceFinancialRecordsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return;

            var alreadyExists = await _context.SellerPayouts
                .AnyAsync(p => p.OrderId == order.Id);

            if (alreadyExists)
                return;

            var commissionRate = await _context.CommissionRates
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            var rate = commissionRate?.RatePercentage ?? 10m;

            foreach (var item in order.OrderItems)
            {
                if (item.Product == null ||
                    string.IsNullOrWhiteSpace(item.Product.SellerId))
                {
                    continue;
                }

                var productTotal = item.Price * item.Quantity;
                var commissionAmount = productTotal * (rate / 100m);
                var sellerEarnings = productTotal - commissionAmount;

                _context.SellerPayouts.Add(new SellerPayout
                {
                    SellerId = item.Product.SellerId,
                    OrderId = order.Id,
                    ProductTotal = productTotal,
                    CommissionAmount = commissionAmount,
                    SellerEarnings = sellerEarnings,
                    CommissionRate = rate,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                });

                _context.PlatformEarnings.Add(new PlatformEarning
                {
                    OrderId = order.Id,
                    ProductTotal = productTotal,
                    CommissionAmount = commissionAmount,
                    CommissionRate = rate,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}