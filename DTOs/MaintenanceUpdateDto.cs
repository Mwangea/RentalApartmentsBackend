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
        public string Status { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }
}
