using Newtonsoft.Json;
using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;
using System.Text;

namespace RentalAppartments.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration  _configuration;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public PaymentService(IConfiguration configuration, ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaymentResult> ProcessMpesaPaymentAsync(MpesaPaymentRequest request)
        {
            try
            {
                // Generate access token
                var accessToken = await GetMpesaAccessTokenAsync();

                //Prepare STK Push request
                var stkPushRequest = new
                {
                    BusinessShortCode = _configuration["Mpesa:BusinessShortCode"],
                    Password = GeneratePassword(),
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    TransactionType = "CustomerPayBillOnline",
                    Amount = request.Amount,
                    PartyA = request.PhoneNumber,
                    PartyB = _configuration["Mpesa:BusinessShortCode"],
                    PhoneNumber = request.PhoneNumber,
                    CallBackURL = _configuration["Mpesa:CallbackUrl"],
                    AccountReference = $"Rent Payment - {request.LeaseId}",
                    TransactionDesc = $"Rent Payment for Lease {request.LeaseId}"
                };

                // Send stk push request
                var stkPushResponse = await SendMpesaRequestAsync("mpesa/stkpush/v1/processrequest", stkPushRequest, accessToken);

                // Create payment record
                var payment = new Payment
                {
                    LeaseId = request.LeaseId,
                    PropertyId = request.PropertyId,
                    TenantId = request.TenantId,
                    Amount = request.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "M-Pesa",
                    TransactionId = stkPushResponse.CheckoutRequestID,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return new PaymentResult
                {
                    Success = true,
                    Message = "Payment initiated successfully",
                    TransactionId = stkPushResponse.CheckoutRequestID
                };
            }
            catch (Exception ex)
            {
                return new PaymentResult
                {
                    Success = false,
                    Message = $"Payment failed: {ex.Message}",
                };
            }

        }


        private async Task <string> GetMpesaAccessTokenAsync()
        {
            var url = $"{_configuration["Mpesa:BaseUrl"]}/oauth/v1/generate?grant_type=client_credentials";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_configuration["Mpesa:ConsumerKey"]}:{_configuration["Mpesa:ConsumerSecret"]}"));

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedCredentials);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<MpesaTokenResponse>(content);

            return tokenResponse.AccessToken;
        }
    }
}
