using RentalAppartments.DTOs;

namespace RentalAppartments.Interfaces
{
    public interface ILeaseService
    {
        Task<IEnumerable<LeaseDto>> GetAllLeasesAsync();
        Task<LeaseDto> GetLeaseAsync(int id, string userId, string role);
        Task<LeaseDto> CreateLeaseAsync(CreateLeaseDto createLeaseDto, string userId, string role);
    }
}
