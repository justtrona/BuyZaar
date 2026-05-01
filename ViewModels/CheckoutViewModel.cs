namespace BuyZaar.ViewModels
{
    public class CheckoutViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductImage { get; set; } = "/images/no-image.png";

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Subtotal { get; set; }

        public string? SelectedVariant { get; set; }

        public string? SelectedSize { get; set; }

        public string ReceiverName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string HouseNo { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string Barangay { get; set; } = string.Empty;

        public string CityMunicipality { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public string Landmark { get; set; } = string.Empty;

        public string DeliveryAddress { get; set; } = string.Empty;
    }
}