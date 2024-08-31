using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Security.Claims;


namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController: ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ISmsService _smsService;

        public NotificationController(INotificationService notificationService, ISmsService smsService)
        {
            _notificationService = notificationService;
            _smsService = smsService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            return Ok(notifications);
        }

        [HttpGet("tenant")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> GetTenantNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notifications = await _notificationService.GetNotificationsByTenantIdAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Landlord,Tenant")]
        public async Task<IActionResult> GetNotification(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            // Check if tenant has access to this notification
            if (User.IsInRole("Tenant"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!await _notificationService.TenantHasAccessToNotificationAsync(userId, id))
                {
                    return Forbid();
                }
            }

            return Ok(notification);
        }

        [HttpPost("send-rent-reminder")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> SendRentReminder([FromBody] RentReminderDto reminderDto)
        {
            var sentNotification = await _notificationService.CreateNotificationAsync(reminderDto);
            await _smsService.SendRentReminderSms(sentNotification);
            return Ok(new { message = "Rent reminder sent successfully", notification = sentNotification });
        }


        [HttpPost("send-maintenance-update")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> SendMaintenanceUpdate([FromBody] MaintenanceUpdateDto updateDto)
        {
            var sentNotification = await _notificationService.CreateNotificationAsync(updateDto);
            await _smsService.SendMaintenanceUpdateSms(sentNotification);
            return Ok(new { message = "Maintenance update sent successfully", notification = sentNotification });
        }

        [HttpPut("settings")]
        [Authorize(Roles = "Tenant,Landlord")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsDto settingsDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedSettings = await _notificationService.UpdateNotificationSettingsAsync(userId, settingsDto);
            return Ok(updatedSettings);
        }

        [HttpGet("settings")]
        [Authorize(Roles = "Tenant,Landlord")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var settings = await _notificationService.GetNotificationSettingsAsync(userId);
            return Ok(settings);
        }

        [HttpPost("test-sms")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTestSms([FromBody] TestSmsDto testSmsDto)
        {
            await _smsService.SendTestSms(testSmsDto.PhoneNumber, testSmsDto.Message);
            return Ok(new { message = "Test SMS sent successfully" });
        }

    }
}
