using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class CreateMaintenanceRequestDto
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }

        public decimal? EstimatedCost { get; set; }

        public bool? IsUrgent { get; set; }
    }
}