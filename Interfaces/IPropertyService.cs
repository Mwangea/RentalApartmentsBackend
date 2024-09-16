using RentalAppartments.DTOs;

namespace RentalAppartments.Interfaces
{
    public interface IPropertyService
    {
        Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync();
        Task<PropertyDto> GetPropertyByIdAsync(int id);
        Task<PropertyDto> AddPropertyAsync(PropertyDto propertyDto, string userId, string role);
        Task<PropertyDto> UpdatePropertyAsync(int id, PropertyDto propertyDto, string userId, string role);
        Task<bool> DeletePropertyAsync(int id, string userId, string role);
        Task<bool> UpdatePropertyImagesAsync(int id, List<string> imageUrls, string userId, string role);
        Task<bool> UpdateRentAmountAsync(int id, decimal newRentAmount, string userId, string role);
        Task<bool> UpdatePropertyStatusAsync(int id, string newStatus, string userId, string role);
        Task<bool> SelectPropertyAsync(int id, string tenantId);
        Task<IEnumerable<PropertyDto>> GetOccupiedPropertiesAsync();
        Task<IEnumerable<PropertyDto>> GetTenantRentedPropertiesAsync(string tenantId);

        Task<string> GetLandlordIdByPropertyIdAsync(int propertyId);
    }
}
