using RentalAppartments.DTOs;

namespace RentalAppartments.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();
        Task<AuthResultDto> ApproveRegistrationAsync(string userId);
        Task<IEnumerable<UserDto>> GetPendingLandlordsAsync();
        Task<IEnumerable<UserDto>> GetAllTenantsAsync();
        Task<IEnumerable<UserDto>> GetAllLandlordsAsync();
        Task<IEnumerable<UserDto>> GetAllAdminsAsync();
    }
}
