using System.ComponentModel.DataAnnotations;

namespace BuyZaar.Models
{
    public class SellerApplicationDocument
    {
        public int Id { get; set; }

        public int SellerApplicationId { get; set; }

        public SellerApplication? SellerApplication { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}