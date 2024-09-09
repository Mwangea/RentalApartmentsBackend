using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class UpdateMaintenanceRequestDto
    {
        [StringLength(100)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
        public decimal? EstimatedCost { get; set; }
        public bool? IsUrgent { get; set; }
    }
}
