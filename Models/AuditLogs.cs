using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string AdminId { get; set; } = string.Empty;

        public ApplicationUser? Admin { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityId { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}