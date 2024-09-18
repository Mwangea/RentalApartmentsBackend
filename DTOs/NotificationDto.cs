using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class NotificationDto
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}
