using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string ShopperId { get; set; } = string.Empty;

        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal ShippingFee { get; set; }

        public string ReceiverName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending Payment";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /*
         * =========================
         * RIDER / DELIVERY SYSTEM
         * =========================
         */

        // Assigned Rider
        public string? RiderId { get; set; }

        public ApplicationUser? Rider { get; set; }

        // Delivery Status
        public string DeliveryStatus { get; set; } = "Pending Assignment";

        // Delivery Timeline
        public DateTime? AssignedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public DateTime? PickedUpAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        /*
         * =========================
         * ORDER ITEMS
         * =========================
         */

        public List<OrderItem> OrderItems { get; set; } = new();

        public DateTime? FailedDeliveryAt { get; set; }

        public DateTime? PreparingAt { get; set; }

public DateTime? ReadyForPickupAt { get; set; }

public bool IsPreparing { get; set; }

public bool IsReadyForPickup { get; set; }

public string? FailedDeliveryReason { get; set; }

public DateTime? ReturnToSellerAt { get; set; }
public string? CancellationRequestStatus { get; set; }

public string? CancellationReason { get; set; }

public DateTime? CancellationRequestedAt { get; set; }

public DateTime? CancellationReviewedAt { get; set; }

public string? CancellationAdminNote { get; set; }
    }
}