using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("mpesa")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> ProcessMpesaPayment([FromBody] MpesaPaymentRequest request)
        {
            // Validate the request body
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract TenantId from user claims
            var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Unauthorized("Unable to retrieve tenant ID from token.");
            }

            // Process payment
            var result = await _paymentService.ProcessMpesaPaymentAsync(request, tenantId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
