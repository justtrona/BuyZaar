using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyZaar.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string ShopperId { get; set; } = string.Empty;

        [ForeignKey(nameof(ShopperId))]
        public ApplicationUser? Shopper { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required]
        [Range(1, 999999)]
        public int Quantity { get; set; } = 1;

        [StringLength(500)]
        public string? SelectedVariant { get; set; }

        [StringLength(500)]
        public string? SelectedSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}