using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyZaar.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 999999)]
        public int Stock { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        // ✅ PRODUCT IMAGES
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        // ✅ SELLER RELATION
        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey(nameof(SellerId))]
        public ApplicationUser? Seller { get; set; }

        // ✅ TIMESTAMPS
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ✅ COLORS / VARIANTS (e.g. Black, White)
        [StringLength(500)]
        public string? AvailableVariants { get; set; }

        // ✅ SIZES (e.g. S, M, L, XL or 36, 37, 38)
        [StringLength(500)]
        public string? AvailableSizes { get; set; }
    }
}