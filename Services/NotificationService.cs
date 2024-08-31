using Microsoft.EntityFrameworkCore;

using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RRentalAppartments.Models;

namespace RentalAppartments.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByTenantIdAsync(string tenantId)
        {
            return await _context.Notifications
                .Where(n => n.UserId.ToString() == tenantId)
                .ToListAsync();
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<bool> TenantHasAccessToNotificationAsync(string tenantId, int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            return notification != null && notification.UserId.ToString() == tenantId;
        }

        public async Task<Notification> CreateNotificationAsync(RentReminderDto reminderDto)
        {
            var notification = new Notification
            {
                UserId = reminderDto.UserId,
                Title = reminderDto.Title,
                Message = reminderDto.Message,
                Type = "RentReminder",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                IsSent = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> CreateNotificationAsync(MaintenanceUpdateDto updateDto)
        {
            var notification = new Notification
            {
                UserId = updateDto.UserId,
                Title = updateDto.Title,
                Message = updateDto.Message,
                Type = "MaintenanceUpdate",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                IsSent = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(string userId, NotificationSettingsDto settingsDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            user.EmailNotifications = settingsDto.EmailNotifications;
            user.SmsNotifications = settingsDto.SmsNotifications;
            user.PushNotifications = settingsDto.PushNotifications;

            await _context.SaveChangesAsync();
            return settingsDto;
        }

        public async Task<NotificationSettingsDto> GetNotificationSettingsAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            return new NotificationSettingsDto
            {
                EmailNotifications = user.EmailNotifications,
                SmsNotifications = user.SmsNotifications,
                PushNotifications = user.PushNotifications
            };
        }
    }
}
