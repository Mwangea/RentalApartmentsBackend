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
        private readonly ILogger<LeaseController> _logger;

        public LeaseController(ILeaseService leaseService, ILogger<LeaseController> logger)
        {
            _leaseService = leaseService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult<LeaseDto>> UpdateLease(int id, UpdateLeaseDto updateLeaseDto)
        {
            if (updateLeaseDto == null)
            {
                return BadRequest("UpdateLeaseDto cannot be null");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return Unauthorized("User identity information is missing");
            }

            try
            {
                var updatedLease = await _leaseService.UpdateLeaseAsync(id, updateLeaseDto, userId, role);
                if (updatedLease == null)
                {
                    return NotFound($"Lease with ID {id} not found");
                }

                return Ok(updatedLease);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating lease {LeaseId}", id);
                return StatusCode(500, "An unexpected error occurred while updating the lease. Please try again later.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult> DeleteLease(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
              var result = await _leaseService.DeleteLeaseAsync(id, userId, role);
                if (!result)
                    return NotFound($"Lease with ID {id} not found.");

                return Ok("Lease deleted successfully.");
            } 
            catch (UnauthorizedAccessException ex) 
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("property/{propertyId}")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult<IEnumerable<LeaseDto>>> GetLeasesByProperty(int propertyId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var leases = await _leaseService.GetLeasesByPropertyAsync(propertyId, userId, role);
                return Ok(leases);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving leases for property {PropertyId}", propertyId);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


        [HttpGet("tenant")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<IEnumerable<LeaseDto>>> GetTenantLease()
        {
            var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var leases = await _leaseService.GetLeasesByTenantAsync(tenantId);
                return Ok(leases);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }



        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult> ActivateLease(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var result = await _leaseService.ActivateLeaseAsync(id, userId, role);
                if (result == null)
                    return NotFound($"Lease with ID {id} not found.");

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult> DeactivateLease(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation($"Attempting to deactivate lease {id} by user {userId} with role {role}");

            try
            {
                var result = await _leaseService.DeactivateLeaseAsync(id, userId, role);
                if (result == null)
                {
                    _logger.LogWarning($"Lease with ID {id} not found");
                    return NotFound($"Lease with ID {id} not found.");
                }

                _logger.LogInformation($"Lease {id} successfully deactivated");
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to deactivate lease {id}: {ex.Message}");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while deactivating lease {id}: {ex.Message}");
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


    }
}
