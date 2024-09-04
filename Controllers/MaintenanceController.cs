using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentalApartmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetAllMaintenanceRequests()
        {
            var requests = await _maintenanceService.GetAllMaintenanceRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("tenant")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetTenantMaintenanceRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var requests = await _maintenanceService.GetMaintenanceRequestsByTenantIdAsync(userId);
            return Ok(requests);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Landlord,Tenant")]
        public async Task<ActionResult<MaintenanceRequest>> GetMaintenanceRequest(int id)
        {
            var request = await _maintenanceService.GetMaintenanceRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Tenant"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (request.TenantId != userId)
                {
                    return Forbid();
                }
            }

            return Ok(request);
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<MaintenanceRequest>> CreateMaintenanceRequest([FromBody] MaintenanceRequestDto requestDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdRequest = await _maintenanceService.CreateMaintenanceRequestAsync(userId, requestDto);
            return CreatedAtAction(nameof(GetMaintenanceRequest), new { id = createdRequest.Id }, createdRequest);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> UpdateMaintenanceRequest(int id, [FromBody] MaintenanceRequestDto requestDto)
        {
            var updatedRequest = await _maintenanceService.UpdateMaintenanceRequestAsync(id, requestDto);
            if (updatedRequest == null)
            {
                return NotFound();
            }
            return Ok(updatedRequest);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> UpdateMaintenanceRequestStatus(int id, [FromBody] string status)
        {
            var updatedRequest = await _maintenanceService.UpdateMaintenanceRequestStatusAsync(id, status);
            if (updatedRequest == null)
            {
                return NotFound();
            }
            return Ok(updatedRequest);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMaintenanceRequest(int id)
        {
            var result = await _maintenanceService.DeleteMaintenanceRequestAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("{id}/update")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> SendMaintenanceUpdate(int id, [FromBody] MaintenanceUpdateDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            updateDto.UserId = userId;
            var result = await _maintenanceService.SendMaintenanceUpdateAsync(id, updateDto);
            if (!result)
            {
                return NotFound();
            }
            return Ok(new { message = "Maintenance update sent successfully" });
        }
    }
}