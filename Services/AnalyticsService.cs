using Microsoft.EntityFrameworkCore;
using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;

namespace RentalAppartments.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsDto> GetAnalyticsForDateAsync(DateTime date)
        {
            var analytics = await _context.Analytics
                .FirstOrDefaultAsync(a => a.Date.Date == date.Date);

            return analytics != null ? MapToDto(analytics) : null;
        }

        public async Task<IEnumerable<AnalyticsDto>> GetAnalyticsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var analytics = await _context.Analytics
                .Where(a => a.Date >= startDate.Date && a.Date <= endDate.Date)
                .OrderBy(a => a.Date)
                .ToListAsync();

            return analytics.Select(MapToDto);
        }

        public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var analytics = await _context.Analytics
                .Where(a => a.Date >= startDate.Date && a.Date <= endDate.Date)
                .ToListAsync();

            return new AnalyticsSummaryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalNewLeases = analytics.Sum(a => a.NewLeasesCount),
                AverageActiveLeases = (int)analytics.Average(a => a.ActiveLeasesCount),
                TotalPaymentsReceived = analytics.Sum(a => a.TotalPaymentsReceived),
                TotalSuccessfulPayments = analytics.Sum(a => a.SuccessfulPaymentsCount),
                TotalFailedPayments = analytics.Sum(a => a.FailedPaymentsCount),
                TotalNewMaintenanceRequests = analytics.Sum(a => a.NewMaintenanceRequestsCount),
                TotalCompletedMaintenanceRequests = analytics.Sum(a => a.CompletedMaintenanceRequestsCount),
                AverageAvailableProperties = (int)analytics.Average(a => a.AvailablePropertiesCount),
                TotalNotificationsSent = analytics.Sum(a => a.NotificationsSentCount),
                AverageUnreadNotifications = (int)analytics.Average(a => a.UnreadNotificationsCount)
            };
        }

        public async Task<AnalyticsDto> RecordDailyAnalyticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var analytics = new Analytics
            {
                Date = today,
                NewLeasesCount = await _context.Leases.CountAsync(l => l.CreatedAt.Date == today),
                ActiveLeasesCount = await _context.Leases.CountAsync(l => l.IsActive),
                TotalPaymentsReceived = await _context.Payments.Where(p => p.PaymentDate.Date == today).SumAsync(p => p.Amount),
                SuccessfulPaymentsCount = await _context.Payments.CountAsync(p => p.PaymentDate.Date == today && p.Status == "Successful"),
                FailedPaymentsCount = await _context.Payments.CountAsync(p => p.PaymentDate.Date == today && p.Status == "Failed"),
                NewMaintenanceRequestsCount = await _context.MaintenanceRequests.CountAsync(m => m.CreatedAt.Date == today),
                CompletedMaintenanceRequestsCount = await _context.MaintenanceRequests.CountAsync(m => m.CompletedAt.HasValue && m.CompletedAt.Value.Date == today),
                AvailablePropertiesCount = await _context.Properties.CountAsync(p => p.IsAvailable),
                NotificationsSentCount = await _context.Notifications.CountAsync(n => n.CreatedAt.Date == today && n.IsSent),
                UnreadNotificationsCount = await _context.Notifications.CountAsync(n => !n.IsRead)
            };

            _context.Analytics.Add(analytics);
            await _context.SaveChangesAsync();

            return MapToDto(analytics);
        }

        public async Task<LeaseAnalyticsDto> GetLeaseAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var leases = await _context.Leases
                .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .ToListAsync();

            return new LeaseAnalyticsDto
            {
                TotalNewLeases = leases.Count,
                AverageLeaseLength = leases.Any() ? leases.Average(l => (l.EndDate - l.StartDate).TotalDays) : 0,
                MostCommonLeaseLength = leases
                    .GroupBy(l => (l.EndDate - l.StartDate).TotalDays)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? 0
            };
        }

        public async Task<PaymentAnalyticsDto> GetPaymentAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .ToListAsync();

            return new PaymentAnalyticsDto
            {
                TotalPaymentsReceived = payments.Sum(p => p.Amount),
                AveragePaymentAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
                SuccessfulPaymentsCount = payments.Count(p => p.Status == "Successful"),
                FailedPaymentsCount = payments.Count(p => p.Status == "Failed")
            };
        }

        public async Task<MaintenanceAnalyticsDto> GetMaintenanceAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var requests = await _context.MaintenanceRequests
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .ToListAsync();

            return new MaintenanceAnalyticsDto
            {
                TotalRequests = requests.Count,
                CompletedRequests = requests.Count(r => r.Status == "Completed"),
                AverageResolutionTime = requests.Any(r => r.CompletedAt.HasValue)
                    ? requests.Where(r => r.CompletedAt.HasValue).Average(r => (r.CompletedAt.Value - r.CreatedAt).TotalHours)
                    : 0,
                MostCommonIssue = requests
                    .GroupBy(r => r.Title)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };
        }

        public async Task<PropertyAnalyticsDto> GetPropertyAnalyticsAsync(DateTime startDate, DateTime endDate, string landlordId)
        {
            var properties = await _context.Properties
                .Where(p => p.LandlordId == landlordId)
                .ToListAsync();

            var occupiedProperties = properties.Count(p => !p.IsAvailable);

            return new PropertyAnalyticsDto
            {
                TotalProperties = properties.Count,
                OccupancyRate = properties.Any() ? (double)occupiedProperties / properties.Count : 0,
                AverageRentAmount = properties.Any() ? properties.Average(p => p.RentAmount) : 0,
                MostCommonPropertyType = properties
                    .GroupBy(p => p.Type)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A",
                PropertyTypeBreakdown = properties
                    .GroupBy(p => p.Type)
                    .Select(g => new PropertyTypeBreakdown
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        AverageRent = g.Average(p => p.RentAmount)
                    })
                    .ToList()
            };
        }

        public async Task<NotificationAnalyticsDto> GetNotificationAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var notifications = await _context.Notifications
                .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
                .ToListAsync();

            return new NotificationAnalyticsDto
            {
                TotalNotificationsSent = notifications.Count(n => n.IsSent),
                ReadRate = notifications.Any() ? (double)notifications.Count(n => n.IsRead) / notifications.Count : 0,
                MostCommonNotificationType = notifications
                    .GroupBy(n => n.Type)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };
        }

        public async Task<UserAnalyticsSummaryDto> GetUserAnalyticsSummaryAsync(string userId, string role)
        {
            var summary = new UserAnalyticsSummaryDto
            {
                UserId = userId,
                Role = role
            };

            switch (role.ToLower())
            {
                case "landlord":
                    summary.TotalProperties = await _context.Properties.CountAsync(p => p.LandlordId == userId);
                    summary.ActiveLeases = await _context.Leases.CountAsync(l => l.Property.LandlordId == userId && l.IsActive);
                    summary.TotalRentCollected = await _context.Payments
                        .Where(p => p.Lease.Property.LandlordId == userId && p.Status == "Successful")
                        .SumAsync(p => p.Amount);
                    break;

                case "tenant":
                    summary.ActiveLeases = await _context.Leases.CountAsync(l => l.TenantId == userId && l.IsActive);
                    summary.TotalRentPaid = await _context.Payments
                        .Where(p => p.Lease.TenantId == userId && p.Status == "Successful")
                        .SumAsync(p => p.Amount);
                    break;
            }

            summary.OpenMaintenanceRequests = await _context.MaintenanceRequests
                .CountAsync(m => (role == "Landlord" ? m.Property.LandlordId == userId : m.TenantId == userId) && m.Status != "Completed");
            summary.CompletedMaintenanceRequests = await _context.MaintenanceRequests
                .CountAsync(m => (role == "Landlord" ? m.Property.LandlordId == userId : m.TenantId == userId) && m.Status == "Completed");
            summary.UnreadNotifications = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return summary;
        }
    

        private AnalyticsDto MapToDto(Analytics analytics)
        {
            return new AnalyticsDto
            {
                Id = analytics.Id,
                Date = analytics.Date,
                NewLeasesCount = analytics.NewLeasesCount,
                ActiveLeasesCount = analytics.ActiveLeasesCount,
                TotalPaymentsReceived = analytics.TotalPaymentsReceived,
                SuccessfulPaymentsCount = analytics.SuccessfulPaymentsCount,
                FailedPaymentsCount = analytics.FailedPaymentsCount,
                NewMaintenanceRequestsCount = analytics.NewMaintenanceRequestsCount,
                CompletedMaintenanceRequestsCount = analytics.CompletedMaintenanceRequestsCount,
                AvailablePropertiesCount = analytics.AvailablePropertiesCount,
                NotificationsSentCount = analytics.NotificationsSentCount,
                UnreadNotificationsCount = analytics.UnreadNotificationsCount
            };
        
        
        }
    }
}
