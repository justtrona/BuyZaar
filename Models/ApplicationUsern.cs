using Microsoft.AspNetCore.Identity;

namespace BuyZaar.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // ✅ Optional custom flag (for UI / reports)
        public bool IsVerified { get; set; } = false;

        // ✅ Optional: track when user verified email
        public DateTime? VerifiedAt { get; set; }

        // Existing relationship
        public ICollection<SellerApplication>? SellerApplications { get; set; }
    }
}