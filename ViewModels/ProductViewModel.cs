using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BuyZaar.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 999999)]
        public int Stock { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AvailableVariants { get; set; }

        [StringLength(500)]
        public string? AvailableSizes { get; set; }

        public List<IFormFile>? ProductImages { get; set; }

        public List<string> ExistingImages { get; set; } = new();
    }
}