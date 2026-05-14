using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class PayMongoWebhookLog
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string? EventType { get; set; }

        public int? OrderId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Received";

        public string? Payload { get; set; }

        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}