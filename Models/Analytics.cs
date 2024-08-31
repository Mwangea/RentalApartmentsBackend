using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.Models
{
    public class Analytics
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; } // The date for which the analytics are recorded

        // Lease-related metrics
        public int NewLeasesCount { get; set; } // Number of new leases signed on the given date
        public int ActiveLeasesCount { get; set; } // Total number of active leases on the given date

        // Payment-related metrics
        public decimal TotalPaymentsReceived { get; set; } // Sum of all payments received on the given date
        public int SuccessfulPaymentsCount { get; set; } // Number of successful payments processed on the given date
        public int FailedPaymentsCount { get; set; } // Number of failed payments processed on the given date

        // Maintenance request-related metrics
        public int NewMaintenanceRequestsCount { get; set; } // Number of new maintenance requests submitted on the given date
        public int CompletedMaintenanceRequestsCount { get; set; } // Number of maintenance requests completed on the given date

        // Property-related metrics
        public int AvailablePropertiesCount { get; set; } // Number of properties available for rent on the given date

        // Notification-related metrics
        public int NotificationsSentCount { get; set; } // Number of notifications sent on the given date
        public int UnreadNotificationsCount { get; set; } // Number of unread notifications at the end of the given date

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp for when this record was created
    }
}
