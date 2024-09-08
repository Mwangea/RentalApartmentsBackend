using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RentalAppartments.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            if (string.IsNullOrWhiteSpace(registerDto.Email))
            {
                return new AuthResultDto { Succeeded = false, Errors = new List<string> { "Email is required." } };
            }

            if (!new EmailAddressAttribute().IsValid(registerDto.Email))
            {
                return new AuthResultDto { Succeeded = false, Errors = new List<string> { "Invalid email format." } };
            }

            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var roleExists = await _roleManager.RoleExistsAsync(registerDto.Role);
                if (!roleExists)
                {
                    return new AuthResultDto { Succeeded = false, Errors = new List<string> { "Role does not exist." } };
                }

                await _userManager.AddToRoleAsync(user, registerDto.Role);

                if (registerDto.Role == "Landlord")
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.MaxValue;
                    await _userManager.UpdateAsync(user);
                    return new AuthResultDto { Succeeded = true, Message = "Registration successful. Awaiting admin approval." };
                }

                return new AuthResultDto { Succeeded = true, Message = "Registration successful." };
            }

            return new AuthResultDto { Succeeded = false, Errors = result.Errors.Select(e => e.Description).ToList() };
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResultDto { Succeeded = false, Errors = new List<string> { "Invalid email or password." } };
            }

            if (user.Role == "Landlord" && user.LockoutEnabled)
            {
                return new AuthResultDto { Succeeded = false, Errors = new List<string> { "Your account is pending approval." } };
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return new AuthResultDto
                {
                    Succeeded = true,
                    Token = token,
                    User = MapToUserDto(user),
                    Role = user.Role
                };
            }

            return new AuthResultDto { Succeeded = false, Errors = new List<string> { "Invalid email or password." } };
        }

        public async Task<AuthResultDto> ApproveRegistrationAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResultDto { Succeeded = false, Errors = new List<string> { "User not found." } };
            }

            user.LockoutEnabled = false;
            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new AuthResultDto { Succeeded = true, User = MapToUserDto(user), Message = "User approved successfully." };
            }

            return new AuthResultDto { Succeeded = false, Errors = result.Errors.Select(e => e.Description).ToList() };
        }

        public async Task<IEnumerable<UserDto>> GetPendingLandlordsAsync()
        {
            var pendingLandlords = await _userManager.GetUsersInRoleAsync("Landlord");
            return pendingLandlords
                .Where(u => u.LockoutEnabled)
                .Select(MapToUserDto);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IEnumerable<UserDto>> GetAllTenantsAsync()
        {
            var tenants = await _userManager.GetUsersInRoleAsync("Tenant");
            return tenants.Select(MapToUserDto);
        }

        public async Task<IEnumerable<UserDto>> GetAllLandlordsAsync()
        {
            var landlords = await _userManager.GetUsersInRoleAsync("Landlord");
            return landlords.Where(u => !u.LockoutEnabled).Select(MapToUserDto);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JwtSettings:JwtKey"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is not configured.");
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:JwtExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:JwtIssuer"],
                audience: _configuration["JwtSettings:JwtIssuer"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            };
        }
    }
}