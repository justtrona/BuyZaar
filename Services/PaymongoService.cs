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

            var secretKey = _configuration["PayMongo:SecretKey"];

            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                var encodedKey = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{secretKey}:")
                );

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", encodedKey);
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
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
        throw new Exception("PayMongo SecretKey is missing.");

    var encodedKey = Convert.ToBase64String(
        Encoding.UTF8.GetBytes($"{secretKey}:")
    );

    _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", encodedKey);

    _httpClient.DefaultRequestHeaders.Accept.Clear();
    _httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );

    var amountInCentavos = Convert.ToInt32(amount * 100);

    var json = $@"
{{
  ""data"": {{
    ""attributes"": {{
      ""send_email_receipt"": true,
      ""show_description"": true,
      ""show_line_items"": true,
      ""description"": {JsonSerializer.Serialize(description)},
      ""line_items"": [
        {{
          ""currency"": ""PHP"",
          ""amount"": {amountInCentavos},
          ""description"": {JsonSerializer.Serialize(description)},
          ""name"": {JsonSerializer.Serialize($"BuyZaar Order #{orderId}")},
          ""quantity"": 1
        }}
      ],
      ""payment_method_types"": [
        ""gcash"",
        ""card"",
        ""paymaya""
      ],
      ""success_url"": {JsonSerializer.Serialize(successUrl)},
      ""cancel_url"": {JsonSerializer.Serialize(failedUrl)},
      ""metadata"": {{
        ""order_id"": {JsonSerializer.Serialize(orderId.ToString())}
      }}
    }}
  }}
}}";

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
        throw new Exception($"PayMongo error: {responseBody}");

    using var document = JsonDocument.Parse(responseBody);

    return document
        .RootElement
        .GetProperty("data")
        .GetProperty("attributes")
        .GetProperty("checkout_url")
        .GetString();
}
    }
}