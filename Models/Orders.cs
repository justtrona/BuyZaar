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

        public string ReceiverName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending Payment";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}