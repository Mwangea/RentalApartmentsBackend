using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalAppartments.Models
{
    public class MaintenanceRequest
    {

        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        [Required]
        public string TenantId { get; set; }

        [ForeignKey("TenantId")]
        public User Tenant { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Pending", "In Progress", "Completed", "Cancelled"

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdated { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        public bool IsUrgent { get; set; }

        [StringLength(255)]
        public string AssignedTo { get; set; } // Could be a staff member or contractor

        public DateTime? ScheduledDate { get; set; }

    }
}
