using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class CommissionRate
    {
        public int Id { get; set; }

        [Range(0, 100)]
        public decimal RatePercentage { get; set; } = 10m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}