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

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey("SellerId")]
        public ApplicationUser? Seller { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}