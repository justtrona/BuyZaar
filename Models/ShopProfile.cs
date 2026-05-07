using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class ShopProfile
    {
        public int Id { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;

        public ApplicationUser? Seller { get; set; }

        [Required]
        [StringLength(100)]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ShopDescription { get; set; }

        [StringLength(150)]
        public string? BusinessEmail { get; set; }

        [StringLength(30)]
        public string? BusinessPhone { get; set; }

        [StringLength(300)]
        public string? ShopAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(100)]
public string? Barangay { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? BusinessHours { get; set; }

        [StringLength(100)]
        public string? ReturnPolicy { get; set; }

        public string? LogoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}