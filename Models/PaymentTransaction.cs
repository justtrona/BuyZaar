using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyZaar.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public int PaymentId { get; set; }

        public Payment? Payment { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public string Provider { get; set; } = "PayMongo";

        public string? ProviderReferenceId { get; set; }

        public string? CheckoutUrl { get; set; }

        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? RawResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }
    }
}