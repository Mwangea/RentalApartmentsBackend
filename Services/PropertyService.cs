using Microsoft.EntityFrameworkCore;
using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;

namespace RentalAppartments.Services
{
    public class PropertyService: IPropertyService

    {
        private readonly ApplicationDbContext _context;

        public PropertyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync()
        {
            var properties = await _context.Properties.ToListAsync();
            return properties.Select(p => MapToDto(p));
        }

        public async Task<PropertyDto> GetPropertyByIdAsync(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            return property != null ? MapToDto(property) : null;
        }

        public async Task<PropertyDto> AddPropertyAsync(PropertyDto propertyDto, string userId, string role)
        {
            if (role != "Admin" && role != "Landlord")
                throw new UnauthorizedAccessException("Only admins and landlords can add properties.");

            var property = new Property
            {
                LandlordId = role == "Landlord" ? userId : propertyDto.LandlordId,
                Name = propertyDto.Name,
                Address = propertyDto.Address,
                Description = propertyDto.Description,
                RentAmount = propertyDto.RentAmount,
                Bedrooms = propertyDto.Bedrooms,
                Bathrooms = propertyDto.Bathrooms,
                SquareFootage = propertyDto.SquareFootage,
                IsAvailable = propertyDto.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return MapToDto(property);
        }

        public async Task<PropertyDto> UpdatePropertyAsync(int id, PropertyDto propertyDto, string userId, string role)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
                return null;

            if (role != "Admin" && (role != "Landlord" || property.LandlordId != userId))
                throw new UnauthorizedAccessException("You don't have permission to update this property.");

            property.Name = propertyDto.Name;
            property.Address = propertyDto.Address;
            property.Description = propertyDto.Description;
            property.RentAmount = propertyDto.RentAmount;
            property.Bedrooms = propertyDto.Bedrooms;
            property.Bathrooms = propertyDto.Bathrooms;
            property.SquareFootage = propertyDto.SquareFootage;
            property.IsAvailable = propertyDto.IsAvailable;
            property.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(property);
        }

        public async Task<bool> DeletePropertyAsync(int id, string userId, string role)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
                return false;

            if (role != "Admin" && (role != "Landlord" || property.LandlordId != userId))
                throw new UnauthorizedAccessException("You don't have permission to delete this property.");

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePropertyImagesAsync(int id, List<string> imageUrls, string userId, string role)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
                return false;

            if (role != "Admin" && (role != "Landlord" || property.LandlordId != userId))
                throw new UnauthorizedAccessException("You don't have permission to update images for this property.");

            // Assuming you have a separate table or field for storing image URLs
            // You'll need to implement this part based on your data model
            // For example:
            // property.ImageUrls = imageUrls;

            property.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateRentAmountAsync(int id, decimal newRentAmount, string userId, string role)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
                return false;

            if (role != "Admin" && (role != "Landlord" || property.LandlordId != userId))
                throw new UnauthorizedAccessException("You don't have permission to update the rent amount for this property.");

            property.RentAmount = newRentAmount;
            property.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePropertyStatusAsync(int id, string newStatus, string userId, string role)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
                return false;

            if (role != "Admin" && (role != "Landlord" || property.LandlordId != userId))
                throw new UnauthorizedAccessException("You don't have permission to update the status of this property.");

            property.Status = newStatus;
            property.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // TODO: Implement notification logic here

            return true;
        }

        public async Task<bool> SelectPropertyAsync(int id, string tenantId)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null || !property.IsAvailable)
                return false;

            property.Status = "Selected";
            property.CurrentTenantId = tenantId;
            property.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // TODO: Implement notification logic here

            return true;
        }

        public async Task<IEnumerable<PropertyDto>> GetOccupiedPropertiesAsync()
        {
            var occupiedProperties = await _context.Properties
                .Where(p => p.Status == "Occupied")
                .ToListAsync();

            return occupiedProperties.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<PropertyDto>> GetTenantRentedPropertiesAsync(string tenantId)
        {
            var rentedProperties = await _context.Properties
                .Where(p => p.CurrentTenantId == tenantId)
                .ToListAsync();

            return rentedProperties.Select(p => MapToDto(p));
        }

        private PropertyDto MapToDto(Property property)
        {
            return new PropertyDto
            {
                Id = property.Id,
                LandlordId = property.LandlordId,
                Name = property.Name,
                Address = property.Address,
                Description = property.Description,
                RentAmount = property.RentAmount,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                SquareFootage = property.SquareFootage,
                IsAvailable = property.IsAvailable,
                CreatedAt = property.CreatedAt,
                LastUpdated = property.LastUpdated,
                // Assuming you have a way to get image URLs
                // ImageUrls = property.ImageUrls
            };
        }
    }
}
