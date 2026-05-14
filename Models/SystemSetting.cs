namespace BuyZaar.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        public bool AllowShopperRegistration { get; set; } = true;
        public bool AllowSellerRegistration { get; set; } = true;
        public bool AllowRiderRegistration { get; set; } = true;

        public bool MaintenanceMode { get; set; } = false;

        public string? MaintenanceMessage { get; set; } =
            "BuyZaar is currently under maintenance. Please come back later.";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}