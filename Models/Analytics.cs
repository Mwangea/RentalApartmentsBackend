using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.Models
{
    public class Analytics
    {
        [Key]
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
        public int UnreadNotificationsCount { get; set; } // Timestamp for when this record was created
    }
}
