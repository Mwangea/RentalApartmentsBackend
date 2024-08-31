namespace RentalAppartments.DTOs
{
    public class AuthResultDto
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; }
        public UserDto User { get; set; }
        public string Role { get; set; }
        public List<string> Errors { get; set; }
        public string Message { get; internal set; }
    }
}
