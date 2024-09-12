using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Security.Claims;

namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController: ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("daily/{date}")]
        public async Task<ActionResult<AnalyticsDto>> GetDailyAnalytics(DateTime date)
        {
            var analytics = await _analyticsService.GetAnalyticsForDateAsync(date);
            if (analytics == null)
                return NotFound();

            return Ok(analytics);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<AnalyticsDto>>> GetAnalyticsRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var analytics = await _analyticsService.GetAnalyticsForDateRangeAsync(startDate, endDate);
            return Ok(analytics);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("summary")]
        public async Task<ActionResult<AnalyticsSummaryDto>> GetAnalyticsSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var summary = await _analyticsService.GetAnalyticsSummaryAsync(startDate, endDate);
            return Ok(summary);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("record-daily")]
        public async Task<ActionResult<AnalyticsDto>> RecordDailyAnalytics()
        {
            var analytics = await _analyticsService.RecordDailyAnalyticsAsync();
            return CreatedAtAction(nameof(GetDailyAnalytics), new { date = analytics.Date }, analytics);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpGet("leases")]
        public async Task<ActionResult<LeaseAnalyticsDto>> GetLeaseAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var leaseAnalytics = await _analyticsService.GetLeaseAnalyticsAsync(startDate, endDate);
            return Ok(leaseAnalytics);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpGet("payments")]
        public async Task<ActionResult<PaymentAnalyticsDto>> GetPaymentAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var paymentAnalytics = await _analyticsService.GetPaymentAnalyticsAsync(startDate, endDate);
            return Ok(paymentAnalytics);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpGet("maintenance")]
        public async Task<ActionResult<MaintenanceAnalyticsDto>> GetMaintenanceAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var maintenanceAnalytics = await _analyticsService.GetMaintenanceAnalyticsAsync(startDate, endDate);
            return Ok(maintenanceAnalytics);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpGet("analytics")]
        public async Task<ActionResult<PropertyAnalyticsDto>> GetPropertyAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var landlordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var propertyAnalytics = await _analyticsService.GetPropertyAnalyticsAsync(startDate, endDate, landlordId);
            return Ok(propertyAnalytics);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("notifications")]
        public async Task<ActionResult<NotificationAnalyticsDto>> GetNotificationAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var notificationAnalytics = await _analyticsService.GetNotificationAnalyticsAsync(startDate, endDate);
            return Ok(notificationAnalytics);
        }

        [Authorize(Roles = "Admin,Landlord,Tenant")]
        [HttpGet("user-summary")]
        public async Task<ActionResult<UserAnalyticsSummaryDto>> GetUserAnalyticsSummary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                return Unauthorized();

            var userSummary = await _analyticsService.GetUserAnalyticsSummaryAsync(userId, role);
            return Ok(userSummary);
        }
    }
}

