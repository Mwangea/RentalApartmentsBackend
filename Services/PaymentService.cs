using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;

namespace RentalAppartments.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly HttpClient _httpClient;

        public PaymentService(ApplicationDbContext context, IConfiguration configuration, ILogger<PaymentService> logger, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PaymentResult> ProcessMpesaPaymentAsync(MpesaPaymentRequest request, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var response = await InitiateMpesaPaymentAsync(request, accessToken);

                // Process the response and save payment details
                var payment = new Payment
                {
                    LeaseId = request.LeaseId,
                    PropertyId = request.PropertyId,
                    TenantId = tenantId, // Use the tenantId passed from the controller
                    Amount = request.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "M-Pesa",
                    TransactionId = response.CheckoutRequestID,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return new PaymentResult
                {
                    Success = true,
                    Message = "Payment initiated successfully",
                    TransactionId = response.CheckoutRequestID
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing M-Pesa payment");
                return new PaymentResult
                {
                    Success = false,
                    Message = "Error processing payment: " + ex.Message
                };
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var consumerKey = _configuration["MpesaSettings:ConsumerKey"];
            var consumerSecret = _configuration["MpesaSettings:ConsumerSecret"];
            var url = _configuration["MpesaSettings:OAuthTokenUrl"];

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<MpesaTokenResponse>(content);

            return tokenResponse.AccessToken;
        }

        private async Task<MpesaPaymentResponse> InitiateMpesaPaymentAsync(MpesaPaymentRequest request, string accessToken)
        {
            var url = _configuration["MpesaSettings:PaymentUrl"];
            var businessShortCode = _configuration["MpesaSettings:BusinessShortCode"];
            var passkey = _configuration["MpesaSettings:Passkey"];
            var callbackUrl = _configuration["MpesaSettings:CallbackUrl"];

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{businessShortCode}{passkey}{timestamp}"));

            var paymentRequest = new
            {
                BusinessShortCode = businessShortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = "CustomerPayBillOnline",
                Amount = request.Amount,
                PartyA = request.PhoneNumber,
                PartyB = businessShortCode,
                PhoneNumber = request.PhoneNumber,
                CallBackURL = callbackUrl,
                AccountReference = $"Rent Payment - Lease {request.LeaseId}",
                TransactionDesc = $"Rent Payment for Property {request.PropertyId}"
            };

            var json = JsonConvert.SerializeObject(paymentRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MpesaPaymentResponse>(responseContent);
        }
    }

    public class MpesaTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
    }

    public class MpesaPaymentResponse
    {
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public string CustomerMessage { get; set; }
    }
}
