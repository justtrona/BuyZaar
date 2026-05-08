using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class RiderProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string AssignedLocation { get; set; } = string.Empty;

        public string? VehicleType { get; set; }

        public string Status { get; set; } = "PendingPasswordSetup";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}