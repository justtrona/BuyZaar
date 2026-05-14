using System.ComponentModel.DataAnnotations.Schema;

namespace BuyZaar.Models
{
    public class PlatformEarning
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionRate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}