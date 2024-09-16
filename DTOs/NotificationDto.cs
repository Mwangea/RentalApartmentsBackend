using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class NotificationDto
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
