using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int LeaseId { get; set; }
        public Lease Lease { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string TenantId { get; set; }
        public User Tenant { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; } // "Credit Card", "Bank Transfer", "Cash", etc.

        public string TransactionId { get; set; }

        [Required]
        public string Status { get; set; } // "Pending", "Completed", "Failed", "Refunded"

        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string Notes { get; set; }

        // Navigation properties
       // public User Tenant { get; set; }
        public Property Property { get; set; }
    }
}
