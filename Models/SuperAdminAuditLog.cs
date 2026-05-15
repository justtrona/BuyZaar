using System.ComponentModel.DataAnnotations.Schema;

namespace BuyZaar.Models
{
    [Table("SuperAdminAuditLogs")]
    public class SuperAdminAuditLog
    {
        public int Id { get; set; }

        public string SuperAdminId { get; set; } = string.Empty;

        [ForeignKey("SuperAdminId")]
        public ApplicationUser? SuperAdmin { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public string EntityId { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}