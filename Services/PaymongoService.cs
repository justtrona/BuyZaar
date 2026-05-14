using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BuyZaar.Services
{
    public class PayMongoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PayMongoService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string?> CreateCheckoutSessionAsync(
            int orderId,
            decimal amount,
            string description,
            string successUrl,
            string failedUrl)
        {
            var secretKey = _configuration["PayMongo:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("PayMongo secret key is missing.");

            var authToken = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{secretKey}:")
            );

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            var amountInCentavos = (int)(amount * 100);

            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                        line_items = new[]
                        {
                            new
                            {
                                currency = "PHP",
                                amount = amountInCentavos,
                                name = $"BuyZaar Order #{orderId}",
                                quantity = 1,
                                description = description
                            }
                        },
                        payment_method_types = new[]
                        {
                            "gcash",
                            "paymaya",
                            "card"
                        },
                        success_url = successUrl,
                        cancel_url = failedUrl,
                        description = $"Payment for BuyZaar Order #{orderId}",
                        reference_number = $"ORDER-{orderId}",
                        send_email_receipt = false,
                        show_description = true,
                        show_line_items = true
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                "https://api.paymongo.com/v1/checkout_sessions",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"PayMongo checkout creation failed: {responseBody}"
                );
            }

            using var document = JsonDocument.Parse(responseBody);

            var checkoutUrl = document
                .RootElement
                .GetProperty("data")
                .GetProperty("attributes")
                .GetProperty("checkout_url")
                .GetString();

            return checkoutUrl;
        }
    }
}