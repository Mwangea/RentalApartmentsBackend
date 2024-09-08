using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Threading.Tasks;

namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);
            if (result.Succeeded)
            {
                return Ok(new { message = result.Message });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result.Succeeded)
            {
                return Ok(new { token = result.Token, user = result.User, role = result.Role });
            }

            return Unauthorized(new { message = "Invalid email or password" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending-landlords")]
        public async Task<IActionResult> GetPendingLandlords()
        {
            var pendingLandlords = await _authService.GetPendingLandlordsAsync();
            return Ok(pendingLandlords);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("approve-landlord/{userId}")]
        public async Task<IActionResult> ApproveLandlord(string userId)
        {
            var result = await _authService.ApproveRegistrationAsync(userId);
            if (result.Succeeded)
            {
                return Ok(new { message = result.Message, user = result.User });
            }

            return BadRequest(result.Errors);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpGet("tenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _authService.GetAllTenantsAsync();
            return Ok(tenants);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("landlords")]
        public async Task<IActionResult> GetAllLandlords()
        {
            var landlords = await _authService.GetAllLandlordsAsync();
            return Ok(landlords);
        }
    }
}