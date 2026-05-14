using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyZaar.Models
{
    public class SellerPayout
    {
        public int Id { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;

        public ApplicationUser? Seller { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellerEarnings { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionRate { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReleasedAt { get; set; }
    }
}