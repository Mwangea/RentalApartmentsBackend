using RentalAppartments.DTOs;
using RRentalAppartments.Models;

namespace RentalAppartments.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllNotificationsAsync();
        Task<IEnumerable<Notification>> GetNotificationsByTenantIdAsync(string tenantId);
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<bool> TenantHasAccessToNotificationAsync(string tenantId, int notificationId);
        Task<Notification> CreateNotificationAsync(RentReminderDto reminderDto);
        Task<Notification> CreateNotificationAsync(MaintenanceUpdateDto updateDto);
        Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(string userId, NotificationSettingsDto settingsDto);
        Task<NotificationSettingsDto> GetNotificationSettingsAsync(string userId);
    }
}
