
using Newtonsoft.Json;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IConfiguration configuration, HttpClient httpClient, ILogger<PaymentService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResult> InitiatePayment(MpesaPaymentRequest request)
    {
        var token = await GetMpesaAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Failed to get access token from M-Pesa");
            return new PaymentResult { Success = false, Message = "Token Generation Failed" };
        }

        var paymentRequest = CreateMpesaPaymentRequest(request);
        var serializedRequest = JsonConvert.SerializeObject(paymentRequest);

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest"),
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", token),
            },
            Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequestMessage);
        var responseString = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var mpesaResponse = JsonConvert.DeserializeObject<MpesaPaymentResponse>(responseString);
            return new PaymentResult { Success = true, Message = mpesaResponse.CustomerMessage, TransactionId = mpesaResponse.CheckoutRequestID };
        }
        else
        {
            _logger.LogError("Payment initiation failed with status code {StatusCode} and message {Message}", response.StatusCode, responseString);
            return new PaymentResult { Success = false, Message = "Payment initiation failed" };
        }
    }

    private async Task<string> GetMpesaAccessTokenAsync()
    {
        var consumerKey = _configuration["Mpesa:ConsumerKey"];
        var consumerSecret = _configuration["Mpesa:ConsumerSecret"];

        if (string.IsNullOrEmpty(consumerKey) || string.IsNullOrEmpty(consumerSecret))
        {
            _logger.LogError("ConsumerKey or ConsumerSecret is not configured.");
            return null;
        }

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials"),
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Basic", credentials)
            }
        };

        var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonConvert.DeserializeObject<MpesaTokenResponse>(content);
            return tokenResponse.AccessToken;
        }
        else
        {
            // Detailed logging for debugging purposes
            _logger.LogError("Failed to get access token from M-Pesa. Status: {StatusCode}, Response: {Response}", response.StatusCode, content);
            return null;
        }
    }

    private object CreateMpesaPaymentRequest(MpesaPaymentRequest request)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var shortCode = _configuration["Mpesa:BusinessShortCode"];
        var passkey = _configuration["Mpesa:Passkey"];
        var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shortCode}{passkey}{timestamp}"));

        return new
        {
            BusinessShortCode = shortCode,
            Password = password,
            Timestamp = timestamp,
            TransactionType = "CustomerPayBillOnline",
            Amount = request.Amount,
            PartyA = request.PhoneNumber, // Tenant's phone number
            PartyB = shortCode, // Business short code
            PhoneNumber = request.PhoneNumber,
            CallBackURL = _configuration["Mpesa:CallBackUrl"],
            AccountReference = request.LeaseId.ToString(),
            TransactionDesc = "Rent Payment"
        };
    }



public class MpesaPaymentResponse
    {
        public string CheckoutRequestID { get; set; }
        public string CustomerMessage { get; set; }
        public string MerchantRequestID { get; set; }
    }

    public class MpesaTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
