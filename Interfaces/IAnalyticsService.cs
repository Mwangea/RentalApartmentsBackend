using RentalAppartments.DTOs;

namespace RentalAppartments.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDto> GetAnalyticsForDateAsync(DateTime date);
        Task<IEnumerable<AnalyticsDto>> GetAnalyticsForDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(DateTime startDate, DateTime endDate);
        Task<AnalyticsDto> RecordDailyAnalyticsAsync();
        Task<LeaseAnalyticsDto> GetLeaseAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<PaymentAnalyticsDto> GetPaymentAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<MaintenanceAnalyticsDto> GetMaintenanceAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<PropertyAnalyticsDto> GetPropertyAnalyticsAsync(DateTime startDate, DateTime endDate, string landlordId);
        Task<NotificationAnalyticsDto> GetNotificationAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<UserAnalyticsSummaryDto> GetUserAnalyticsSummaryAsync(string userId, string role);
    }
}
