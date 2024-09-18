
using Newtonsoft.Json;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RentalAppartments.Models;
using RentalAppartments.Data;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public PaymentService(IConfiguration configuration, HttpClient httpClient, ILogger<PaymentService> logger, ApplicationDbContext dbContext, INotificationService notificationService)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
        _context = dbContext;
        _notificationService = notificationService;
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

            // save the initial payment record
            await SaveInitialPaymentRecord(request, mpesaResponse.CheckoutRequestID);

            return new PaymentResult
            {
                Success = true,
                Message = mpesaResponse.CustomerMessage,
                TransactionId = mpesaResponse.CheckoutRequestID,
                MpesaTransactionId = mpesaResponse.MpesaReceiptNumber  // Set the M-Pesa transaction ID
            };
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

    private async Task SaveInitialPaymentRecord(MpesaPaymentRequest request, string checkoutRequestId)
    {
        var payment = new Payment
        {
           LeaseId = request.LeaseId,
           PropertyId = request.PropertyId,
           TenantId = request.TenantId,
           Amount = request.Amount,
           PaymentDate = DateTime.UtcNow,
           TransactionId = checkoutRequestId,
           Status = "Pending",
           CreatedAt = DateTime.UtcNow,
           Notes = "Initial payment record",
           PaymentMethod = "M-Pesa"
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<PaymentResult> InitiateCashPayment(CashPaymentRequest request)
    {
        var payment = new Payment
        {
            LeaseId = request.LeaseId,
            PropertyId = request.PropertyId,
            TenantId = request.TenantId,
            Amount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            TransactionId = Guid.NewGuid().ToString(), // Generate a unique ID for cash payments
            Status = "Completed", // Cash payments are typically completed immediately
            CreatedAt = DateTime.UtcNow,
            Notes = "Cash payment",
            PaymentMethod = "Cash"
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentResult { Success = true, Message = "Cash payment recorded successfully", TransactionId = payment.TransactionId };
    }

    public async Task<bool> HandleMpesaCallbackAsync(MpesaCallbackDto callbackData)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == callbackData.CheckoutRequestID);

        if (payment == null)
        {
            _logger.LogWarning("Received callback for unknown payment: {CheckoutRequestID}", callbackData.CheckoutRequestID);
            return false;
        }

        payment.Status = callbackData.ResultCode == "0" ? "Completed" : "Failed";
        payment.LastUpdated = DateTime.UtcNow;
        payment.MpesaTransactionId = callbackData.MpesaReceiptNumber;  // Set the M-Pesa transaction ID
        payment.Notes = $"M-Pesa Receipt: {callbackData.MpesaReceiptNumber}";

        await _context.SaveChangesAsync();

        if (payment.Status == "Completed")
        {
            await NotifyPaymentSuccess(payment);
        }

        return true;
    }

    private async Task NotifyPaymentSuccess(Payment payment)
    {
        var tenant = await _context.Users.FindAsync(payment.TenantId);
        var property = await _context.Properties.Include(p => p.Landlord).FirstOrDefaultAsync(p => p.Id == payment.PropertyId);
        var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();

        if (tenant != null)
        {
            await _notificationService.CreateGeneralNotificationAsync(new NotificationDto
            {
                UserId = tenant.Id,
                Title = "Payment Successful",
                Message = $"Your payment of {payment.Amount} was successful. M-Pesa Receipt: {payment.Notes.Replace("M-Pesa Receipt: ", "")}",
                Type = "PaymentConfirmation"
            });
        }

        if (property?.Landlord != null)
        {
            await _notificationService.CreateGeneralNotificationAsync(new NotificationDto
            {
                UserId = property.Landlord.Id,
                Title = "Payment Received",
                Message = $"Payment of {payment.Amount} received for property {property.Name}. M-Pesa Receipt: {payment.Notes.Replace("M-Pesa Receipt: ", "")}",
                Type = "PaymentConfirmation"
            });
        }

        foreach (var admin in admins)
        {
            await _notificationService.CreateGeneralNotificationAsync(new NotificationDto
            {
                UserId = admin.Id,
                Title = "New Payment Processed",
                Message = $"A payment of {payment.Amount} has been processed for property ID {payment.PropertyId}. M-Pesa Receipt: {payment.Notes.Replace("M-Pesa Receipt: ", "")}",
                Type = "PaymentConfirmation"
            });
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


    public async Task<Payment> GetPaymentByTransactionIdAsync(string transactionId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }

    public async Task<IEnumerable<object>> GetSuccessfulPaymentsForAdmin()
    {
        return await _context.Payments
            .Where(p => p.Status == "Completed")
            .Select(p => new
            {
                MpesaCode = p.Notes.Replace("M-Pesa Receipt: ", ""),
                Amount = p.Amount,
                Date = p.PaymentDate
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetSuccessfulPaymentsForLandlord(string landlordId, int propertyId)
    {
        return await _context.Payments
            .Where(p => p.Property.LandlordId == landlordId && p.PropertyId == propertyId && p.Status == "Completed")
            .Select(p => new
            {
                MpesaCode = p.Notes.Replace("M-Pesa Receipt: ", ""),
                Amount = p.Amount,
                Date = p.PaymentDate
            })
            .OrderByDescending(p => p.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsForTenantAsync(string tenantId)
    {
        return await _context.Payments
            .Where(p => p.Status == "Completed" && p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }



    public class MpesaPaymentResponse
    {
        public string CheckoutRequestID { get; set; }
        public string CustomerMessage { get; set; }
        public string MerchantRequestID { get; set; }
        public string MpesaReceiptNumber { get; set; }
    }

    public class MpesaTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
