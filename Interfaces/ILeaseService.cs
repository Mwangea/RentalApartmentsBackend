using RentalAppartments.DTOs;

namespace RentalAppartments.Interfaces
{
    public interface ILeaseService
    {
        Task<IEnumerable<LeaseDto>> GetAllLeasesAsync();
        Task<LeaseDto> GetLeaseAsync(int id, string userId, string role);
        Task<LeaseDto> CreateLeaseAsync(CreateLeaseDto createLeaseDto, string userId, string role);
        Task<LeaseDto> UpdateLeaseAsync(int id, UpdateLeaseDto updateLeaseDto, string userId, string role);
        Task<bool> DeleteLeaseAsync(int id, string userId, string role);
        Task<IEnumerable<LeaseDto>> GetLeasesByPropertyAsync(int propertyId, string userId, string role);
        Task<IEnumerable<LeaseDto>> GetLeasesByTenantAsync(string tenantId);
        Task<string> ActivateLeaseAsync(int id, string userId, string role);
        Task<string> DeactivateLeaseAsync(int id, string userId, string role);
    }
}
