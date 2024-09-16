using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using RentalAppartments.Models;

namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly IPropertyService _propertyService;
        private readonly IAuthService _authService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            INotificationService notificationService,
            IPropertyService propertyService,
            IAuthService authService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _notificationService = notificationService;
            _propertyService = propertyService;
            _authService = authService;
            _logger = logger;
        }



        [HttpPost("mpesa")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> ProcessPayment([FromBody] MpesaPaymentRequest paymentRequest)
        {
            try
            {
                var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return Unauthorized("Unable to retrieve tenant ID from token.");
                }

                // Set the TenantId from the authenticated user's claims
                paymentRequest.TenantId = tenantId;

                _logger.LogInformation("Processing payment for tenant: {TenantId}", tenantId);

                var paymentResult = await _paymentService.InitiatePayment(paymentRequest);

                if (paymentResult.Success)
                {
                    await NotifyTenant(paymentRequest, paymentResult.TransactionId);
                    await NotifyLandlord(paymentRequest, paymentResult.TransactionId);
                    await NotifyAdmins(paymentRequest, paymentResult.TransactionId);

                    return Ok(paymentResult);
                }

                if (paymentResult.Cancelled)
                {
                    return Ok(new { Message = "Payment was cancelled by the user.", paymentResult.TransactionId });
                }

                return BadRequest(paymentResult.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing payment for tenant {TenantId}", paymentRequest.TenantId);
                return StatusCode(500, "An error occurred while processing your payment. Please try again later.");
            }
        }

        private async Task NotifyTenant(MpesaPaymentRequest paymentRequest, string transactionId)
        {
            try
            {
                await _notificationService.CreateNotificationAsync(new RentReminderDto
                {
                    UserId = paymentRequest.TenantId,
                    Title = "Payment Successful",
                    Message = $"Your payment of {paymentRequest.Amount} was successful. Transaction ID: {transactionId}"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to create notification for tenant {TenantId}. Transaction ID: {TransactionId}", paymentRequest.TenantId, transactionId);
            }
        }

        private async Task NotifyLandlord(MpesaPaymentRequest paymentRequest, string transactionId)
        {
            try
            {
                var landlordId = await _propertyService.GetLandlordIdByPropertyIdAsync(paymentRequest.PropertyId);
                if (landlordId != null)
                {
                    await _notificationService.CreateNotificationAsync(new RentReminderDto
                    {
                        UserId = landlordId,
                        Title = "Payment Received",
                        Message = $"Payment of {paymentRequest.Amount} has been received for property ID {paymentRequest.PropertyId}. Transaction ID: {transactionId}"
                    });
                }
                else
                {
                    _logger.LogWarning("Landlord not found for property {PropertyId}. Transaction ID: {TransactionId}", paymentRequest.PropertyId, transactionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify landlord for property {PropertyId}. Transaction ID: {TransactionId}", paymentRequest.PropertyId, transactionId);
            }
        }

        private async Task NotifyAdmins(MpesaPaymentRequest paymentRequest, string transactionId)
        {
            try
            {
                var admins = await _authService.GetAllAdminsAsync();
                foreach (var admin in admins)
                {
                    await _notificationService.CreateNotificationAsync(new RentReminderDto
                    {
                        UserId = admin.Id,
                        Title = "New Payment Processed",
                        Message = $"A payment of {paymentRequest.Amount} has been processed for property ID {paymentRequest.PropertyId}. Transaction ID: {transactionId}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify admins for payment. Transaction ID: {TransactionId}", transactionId);
            }
        }
    }
}