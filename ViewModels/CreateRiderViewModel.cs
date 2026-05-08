using System.ComponentModel.DataAnnotations;

namespace BuyZaar.ViewModels
{
    public class CreateRiderViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Assigned Location")]
        public string AssignedLocation { get; set; } = string.Empty;

        [Display(Name = "Vehicle Type")]
        public string? VehicleType { get; set; }
    }
}