using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        [Required]
        public string TenantId { get; set; }
        public User Tenant { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } // "Pending", "In Progress", "Completed", "Cancelled"

        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string Notes { get; set; }

        public decimal? Cost { get; set; }
    }
}
