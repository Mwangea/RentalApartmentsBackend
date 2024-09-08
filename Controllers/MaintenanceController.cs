using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // Import the ILogger
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;
using RentalAppartments.Services;
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
        private readonly ILogger<MaintenanceController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IPropertyService _propertyService;

        public MaintenanceController(
            IMaintenanceService maintenanceService,
            ILogger<MaintenanceController> logger,
            UserManager<User> userManager,
            IPropertyService propertyService)
        {
            _maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<ActionResult<MaintenanceRequestsResponse>> GetAllMaintenanceRequests([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var requests = await _maintenanceService.GetAllMaintenanceRequestsAsync();
                var totalCount = requests.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var pagedRequests = requests
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new MaintenanceRequestsResponse
                {
                    TotalCount = totalCount,
                    Requests = pagedRequests.Select(r => new MaintenanceRequestDto
                    {
                        Id = r.Id,
                        PropertyId = r.PropertyId,
                        PropertyName = r.Property.Name,
                        TenantId = r.TenantId,
                        TenantName = $"{r.Tenant?.FirstName} {r.Tenant?.LastName}",
                        Title = r.Title,
                        Description = r.Description,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt,
                        LastUpdated = r.LastUpdated,
                        CompletedAt = r.CompletedAt,
                        Notes = r.Notes,
                        EstimatedCost = r.Cost,
                        IsUrgent = r.IsUrgent
                    }).ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving maintenance requests.");
                return StatusCode(500, new { message = "An error occurred while processing your request. Please try again later." });
            }
        }

        public class MaintenanceRequestsResponse
        {
            public int TotalCount { get; set; }
            public List<MaintenanceRequestDto> Requests { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public bool HasNextPage { get; set; }
            public bool HasPreviousPage { get; set; }
        }

        [HttpGet("tenant")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequestDto>>> GetTenantMaintenanceRequests()
        {
            var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest("Unable to identify the tenant.");
            }

            var requests = await _maintenanceService.GetMaintenanceRequestsByTenantIdAsync(tenantId);

            var requestDtos = requests.Select(r => new MaintenanceRequestDto
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                PropertyName = r.Property?.Name,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                LastUpdated = r.LastUpdated,
                CompletedAt = r.CompletedAt,
                Notes = r.Notes,
                EstimatedCost = r.Cost,
                IsUrgent = r.IsUrgent
            }).ToList();

            return Ok(requestDtos);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Landlord")]
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

            var maintenanceRequestDto = new MaintenanceRequestDto
            {
                Id = request.Id,
                PropertyId = request.PropertyId,
                PropertyName = request.Property.Name,
                TenantName = $"{request.Tenant?.FirstName} {request.Tenant?.LastName}",
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                LastUpdated = request.LastUpdated,
                CompletedAt = request.CompletedAt,
                Notes = request.Notes,
                EstimatedCost = request.Cost,
                IsUrgent = request.IsUrgent
            };

            return Ok(maintenanceRequestDto);
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestDto createDto)
        {
            try
            {
                // Log the received request
              //  _logger.LogInformation($"Received create maintenance request: {System.Text.Json.JsonSerializer.Serialize(createDto)}");

                // Check if the model is valid
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    return BadRequest(ModelState);
                }

                // Get the tenant's ID from the JWT token
                var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(tenantId))
                {
                    _logger.LogWarning("User ID is missing from the claims.");
                    return BadRequest("Unable to identify the tenant.");
                }

                // Get the tenant from the database
                var tenant = await _userManager.FindByIdAsync(tenantId);
                if (tenant == null)
                {
                    _logger.LogWarning($"Tenant with ID {tenantId} not found.");
                    return BadRequest("Invalid tenant. Tenant does not exist.");
                }

                // Get the property details
                var property = await _propertyService.GetPropertyByIdAsync(createDto.PropertyId);
                if (property == null)
                {
                    _logger.LogWarning($"Property with ID {createDto.PropertyId} not found.");
                    return BadRequest("Invalid property ID.");
                }

                // Create the MaintenanceRequestDto
                var requestDto = new MaintenanceRequestDto
                {
                    PropertyId = createDto.PropertyId,
                    TenantId = tenantId,
                    TenantName = $"{tenant.FirstName} {tenant.LastName}",
                    PropertyName = property.Name,
                    Title = createDto.Title,
                    Description = createDto.Description,
                    Status = createDto.Status ?? "Pending",
                    Notes = createDto.Notes,
                    EstimatedCost = createDto.EstimatedCost,
                    IsUrgent = createDto.IsUrgent ?? false
                };

                // Create the maintenance request
               // _logger.LogInformation($"Creating maintenance request for tenant ID: {tenantId}");

                var createdRequest = await _maintenanceService.CreateMaintenanceRequestAsync(requestDto);

                // Return a simplified success message instead of full object
                return Ok(new { message = "Maintenance request created successfully", requestId = createdRequest.Id });
            }
            catch (Exception ex)
            {
                // Log the error and return a server error response
                _logger.LogError(ex, "An error occurred while creating a maintenance request.");
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
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
