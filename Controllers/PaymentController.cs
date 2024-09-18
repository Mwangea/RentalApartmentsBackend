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

                paymentRequest.TenantId = tenantId;

                if (!TryValidateModel(paymentRequest))
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Processing payment for tenant: {TenantId}", tenantId);

                var paymentResult = await _paymentService.InitiatePayment(paymentRequest);

                if (paymentResult.Success)
                {
                    await NotifyTenant(paymentRequest, paymentResult.MpesaTransactionId);
                    await NotifyLandlord(paymentRequest, paymentResult.MpesaTransactionId);
                    await NotifyAdmins(paymentRequest, paymentResult.MpesaTransactionId);

                    return Ok(paymentResult);
                }

                if (paymentResult.Cancelled)
                {
                    return Ok(new { Message = "Payment was cancelled by the user.", paymentResult.MpesaTransactionId });
                }

                return BadRequest(paymentResult.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing payment for tenant {TenantId}", paymentRequest.TenantId);
                return StatusCode(500, "An error occurred while processing your payment. Please try again later.");
            }
        }

        [HttpGet("successful-payments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSuccessfulPayments()
        {
            try
            {
                var payments = await _paymentService.GetSuccessfulPaymentsForAdmin();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve successful payments for admin.");
                return StatusCode(500, "An error occurred while retrieving payments.");
            }
        }

        [HttpGet("successful-rent")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> GetSuccessfulPaymentsForLandlord(int propertyId)
        {
            try
            {
                var landlordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(landlordId))
                {
                    return Unauthorized("Unable to retrieve landlord ID from token.");
                }

                var payments = await _paymentService.GetSuccessfulPaymentsForLandlord(landlordId, propertyId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve successful payments for landlord.");
                return StatusCode(500, "An error occurred while retrieving payments.");
            }
        }


        [HttpGet("all-payments")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> GetPaymentsForTenant()
        {
            var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Unauthorized("Unable to retrieve tenant ID from token.");
            }

            var payments = await _paymentService.GetPaymentsForTenantAsync(tenantId);
            if (payments == null || !payments.Any())
            {
                return NotFound("No payments found for the tenant.");
            }

            return Ok(payments);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> HandleMpesaCallback([FromBody] MpesaCallbackDto callbackData)
        {
            try
            {
                _logger.LogInformation("Received M-Pesa callback: {@CallbackData}", callbackData);

                var success = await _paymentService.HandleMpesaCallbackAsync(callbackData);

                if (success)
                {
                    return Ok(new { ResultCode = "0", ResultDesc = "Success" });
                }
                else
                {
                    return BadRequest(new { ResultCode = "1", ResultDesc = "Payment not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing M-Pesa callback");
                return StatusCode(500, new { ResultCode = "1", ResultDesc = "Internal Server Error" });
            }
        }

        [HttpPost("cash")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> ProcessCashPayment([FromBody] CashPaymentRequest paymentRequest)
        {
            try
            {
                var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return Unauthorized("Unable to retrieve tenant ID from token.");
                }

                paymentRequest.TenantId = tenantId;

                if (!TryValidateModel(paymentRequest))
                {
                    return BadRequest(ModelState);
                }

                var paymentResult = await _paymentService.InitiateCashPayment(paymentRequest);

                if (paymentResult.Success)
                {
                    await NotifyTenant(paymentRequest, paymentResult.TransactionId);
                    await NotifyLandlord(paymentRequest, paymentResult.TransactionId);
                    await NotifyAdmins(paymentRequest, paymentResult.TransactionId);

                    return Ok(paymentResult);
                }

                return BadRequest(paymentResult.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing cash payment for tenant {TenantId}", paymentRequest.TenantId);
                return StatusCode(500, "An error occurred while processing your payment. Please try again later.");
            }
        }


        private async Task NotifyTenant(IPaymentRequest paymentRequest, string transactionId)
        {
            try
            {
                await _notificationService.CreateNotificationAsync(new RentReminderDto
                {
                    UserId = paymentRequest.TenantId,
                    Title = "Payment Successful",
                    Message = $"Your payment of {paymentRequest.Amount} was successful. M-Pesa Transaction ID: {transactionId}"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to create notification for tenant {TenantId}. M-Pesa Transaction ID: {TransactionId}", paymentRequest.TenantId, transactionId);
            }
        }

        private async Task NotifyLandlord(IPaymentRequest paymentRequest, string transactionId)
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
                        Message = $"Payment of {paymentRequest.Amount} has been received for property ID {paymentRequest.PropertyId}. M-Pesa Transaction ID: {transactionId}"
                    });
                }
                else
                {
                    _logger.LogWarning("Landlord not found for property {PropertyId}. M-Pesa Transaction ID: {TransactionId}", paymentRequest.PropertyId, transactionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify landlord for property {PropertyId}. M-Pesa Transaction ID: {TransactionId}", paymentRequest.PropertyId, transactionId);
            }
        }

        private async Task NotifyAdmins(IPaymentRequest paymentRequest, string transactionId)
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
                        Message = $"A payment of {paymentRequest.Amount} has been processed for property ID {paymentRequest.PropertyId}. M-Pesa Transaction ID: {transactionId}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify admins for payment. M-Pesa Transaction ID: {TransactionId}", transactionId);
            }
        }
    }
}