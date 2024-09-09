using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class MaintenanceUpdateDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? ScheduledDate { get; set; }

        // Additional properties that might be useful
        public int? MaintenanceRequestId { get; set; }

        public decimal? UpdatedCost { get; set; }

        [StringLength(500)]
        public string AdditionalNotes { get; set; }

        public bool? IsUrgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
