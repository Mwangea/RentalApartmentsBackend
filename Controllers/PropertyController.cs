using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using System.Security.Claims;

namespace RentalAppartments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController: ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PropertyDto>>> GetAllProperties()
        {
            var properties = await _propertyService.GetAllPropertiesAsync();
            return Ok(properties);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PropertyDto>> GetProperty(int id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null)
                return NotFound();

            return Ok(property);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpPost]
        public async Task<ActionResult<PropertyDto>> AddProperty(PropertyDto propertyDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var newProperty = await _propertyService.AddPropertyAsync(propertyDto, userId, role);
            return CreatedAtAction(nameof(GetProperty), new { id = newProperty.Id }, newProperty);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProperty(int id, PropertyDto propertyDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var updatedProperty = await _propertyService.UpdatePropertyAsync(id, propertyDto, userId, role);
            if (updatedProperty == null)
                return NotFound();

            return Ok(updatedProperty);
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _propertyService.DeletePropertyAsync(id, userId, role);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpPut("{id}/images")]
        public async Task<IActionResult> UpdatePropertyImages(int id, [FromBody] List<string> imageUrls)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _propertyService.UpdatePropertyImagesAsync(id, imageUrls, userId, role);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpPut("{id}/rent")]
        public async Task<IActionResult> UpdateRentAmount(int id, [FromBody] decimal newRentAmount)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _propertyService.UpdateRentAmountAsync(id, newRentAmount, userId, role);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePropertyStatus(int id, [FromBody] string newStatus)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _propertyService.UpdatePropertyStatusAsync(id, newStatus, userId, role);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Tenant")]
        [HttpPost("{id}/select")]
        public async Task<IActionResult> SelectProperty(int id)
        {
            var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _propertyService.SelectPropertyAsync(id, tenantId);
            if (!result)
                return BadRequest("Property not available or not found.");

            return Ok();
        }

        [Authorize(Roles = "Admin,Landlord")]
        [HttpGet("occupied")]
        public async Task<ActionResult<IEnumerable<PropertyDto>>> GetOccupiedProperties()
        {
            var occupiedProperties = await _propertyService.GetOccupiedPropertiesAsync();
            return Ok(occupiedProperties);
        }

        [Authorize(Roles = "Tenant")]
        [HttpGet("rented")]
        public async Task<ActionResult<IEnumerable<PropertyDto>>> GetTenantRentedProperties()
        {
            var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rentedProperties = await _propertyService.GetTenantRentedPropertiesAsync(tenantId);
            return Ok(rentedProperties);
        }
    }
}
