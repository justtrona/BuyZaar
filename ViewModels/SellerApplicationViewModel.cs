using System.ComponentModel.DataAnnotations;

namespace BuyZaar.ViewModels
{
    public class SellerApplicationViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Business Description")]
        public string BusinessDescription { get; set; } = string.Empty;
    }
}