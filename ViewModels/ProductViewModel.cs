using System.ComponentModel.DataAnnotations;

namespace BuyZaar.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 999999)]
        public int Stock { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;
    }
}