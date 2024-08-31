using RentalAppartments.Models;
using System.ComponentModel.DataAnnotations;

namespace RRentalAppartments.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Type { get; set; } // "RentReminder", "MaintenanceUpdate", "PaymentConfirmation", etc.

        public bool IsRead { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }

        // For SMS notifications
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
