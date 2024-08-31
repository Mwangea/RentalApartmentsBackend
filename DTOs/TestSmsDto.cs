using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class TestSmsDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
