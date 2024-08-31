using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [CustomEmailValidation(ErrorMessage = "Email cannot be empty or whitespace.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; }
    }

    public class CustomEmailValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is string email)
            {
                return !string.IsNullOrWhiteSpace(email);
            }
            return false;
        }
    }
}