using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Services
{
    /// <summary>
    /// Verifies Google reCAPTCHA v2 tokens by calling the Google siteverify API.
    /// The secret key is read from configuration and never exposed to the frontend.
    /// </summary>
    public class RecaptchaService : IRecaptchaService
    {
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;

        public RecaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _secretKey = configuration["Recaptcha:SecretKey"]
                ?? throw new InvalidOperationException("Recaptcha:SecretKey is not configured.");
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _secretKey),
                    new KeyValuePair<string, string>("response", token)
                })
            );

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty("success", out var success) && success.GetBoolean();
        }
    }
}
