namespace RentalAppartments.DTOs
{
    public class AnalyticsDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int NewLeasesCount { get; set; }
        public int ActiveLeasesCount { get; set; }
        public decimal TotalPaymentsReceived { get; set; }
        public int SuccessfulPaymentsCount { get; set; }
        public int FailedPaymentsCount { get; set; }
        public int NewMaintenanceRequestsCount { get; set; }
        public int CompletedMaintenanceRequestsCount { get; set; }
        public int AvailablePropertiesCount { get; set; }
        public int NotificationsSentCount { get; set; }
        public int UnreadNotificationsCount { get; set; }
    }

    public class AnalyticsSummaryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalNewLeases { get; set; }
        public int AverageActiveLeases { get; set; }
        public decimal TotalPaymentsReceived { get; set; }
        public int TotalSuccessfulPayments { get; set; }
        public int TotalFailedPayments { get; set; }
        public int TotalNewMaintenanceRequests { get; set; }
        public int TotalCompletedMaintenanceRequests { get; set; }
        public int AverageAvailableProperties { get; set; }
        public int TotalNotificationsSent { get; set; }
        public int AverageUnreadNotifications { get; set; }
    }

    public class LeaseAnalyticsDto
    {
        public int TotalNewLeases { get; set; }
        public double AverageLeaseLength { get; set; }
        public double MostCommonLeaseLength { get; set; }
    }

    public class PaymentAnalyticsDto
    {
        public decimal TotalPaymentsReceived { get; set; }
        public decimal AveragePaymentAmount { get; set; }
        public int SuccessfulPaymentsCount { get; set; }
        public int FailedPaymentsCount { get; set; }
    }

    public class MaintenanceAnalyticsDto
    {
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public double AverageResolutionTime { get; set; }
        public string MostCommonIssue { get; set; }
    }

    public class PropertyAnalyticsDto
    {
        public int TotalProperties { get; set; }
        public double OccupancyRate { get; set; }
        public decimal AverageRentAmount { get; set; }
        public string MostCommonPropertyType { get; set; }
        public List<PropertyTypeBreakdown> PropertyTypeBreakdown { get; set; }
    }

    public class NotificationAnalyticsDto
    {
        public int TotalNotificationsSent { get; set; }
        public double ReadRate { get; set; }
        public string MostCommonNotificationType { get; set; }
    }

    public class UserAnalyticsSummaryDto
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public int TotalProperties { get; set; }  // For Landlords
        public int ActiveLeases { get; set; }
        public decimal TotalRentCollected { get; set; }  // For Landlords
        public decimal TotalRentPaid { get; set; }  // For Tenants
        public int OpenMaintenanceRequests { get; set; }
        public int CompletedMaintenanceRequests { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class PropertyTypeBreakdown
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public decimal AverageRent { get; set; }
    }
}
