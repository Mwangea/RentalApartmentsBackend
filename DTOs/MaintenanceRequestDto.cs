using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class MaintenanceRequestDto
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Notes { get; set; }

        public decimal? EstimatedCost { get; set; }
    }
}
