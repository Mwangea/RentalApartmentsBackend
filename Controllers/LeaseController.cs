using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Security.Claims;

namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaseController: ControllerBase
    {
        private readonly ILeaseService _leaseService;

        public LeaseController(ILeaseService leaseService)
        {
            _leaseService = leaseService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LeaseDto>>> GetAllLeases()
        {
            var leases = await _leaseService.GetAllLeasesAsync();
            return Ok(leases);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Landlord,Tenant")]
        public async Task <ActionResult<LeaseDto>> GetLease(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var lease = await _leaseService.GetLeaseAsync(id, userId, role);
                if (lease == null)
                    return NotFound();

                return Ok(lease);
            }

            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult<LeaseDto>> CreateLease(CreateLeaseDto createLeaseDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var createdLease = await _leaseService.CreateLeaseAsync(createLeaseDto, userId, role);
                return CreatedAtAction(nameof(GetLease), new { id = createdLease.Id }, createdLease);
            }

            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex) 
            {
                return BadRequest($"An error occured: {ex.Message}");
            }
        }
    }
}
