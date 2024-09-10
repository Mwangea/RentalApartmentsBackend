using Microsoft.EntityFrameworkCore;
using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;

namespace RentalAppartments.Services
{
    public class LeaseService : ILeaseService
    {
        private readonly ApplicationDbContext _context;

        public LeaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeaseDto> GetLeaseAsync(int id, string userId, string role)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .ThenInclude(p => p.Landlord)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null)
                return null;

            // Add authorization logic here
            if (role != "Admin" && lease.Tenant.Id != userId && lease.Property.LandlordId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this lease.");
            }

            return MapLeaseToDto(lease);
        }

        public async Task<IEnumerable<LeaseDto>> GetAllLeasesAsync()
        {
            var leases = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .ToListAsync();

            return leases.Select(MapLeaseToDto);
        }

        private LeaseDto MapLeaseToDto(Lease lease)
        {
            string landlordName = $"{lease.Property?.Landlord?.FirstName} {lease.Property?.Landlord?.LastName}".Trim();
            string tenantName = $"{lease.Tenant?.FirstName} {lease.Tenant?.LastName}".Trim();

            return new LeaseDto
            {
                Id = lease.Id,
                PropertyId = lease.PropertyId,
                PropertyName = lease.Property?.Name,
                LandlordId = lease.Property?.LandlordId,
                LandlordName = string.IsNullOrWhiteSpace(landlordName) ? null : landlordName,
                TenantId = lease.TenantId,
                TenantName = string.IsNullOrWhiteSpace(tenantName) ? null : tenantName,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                MonthlyRent = lease.MonthlyRent,
                SecurityDeposit = lease.SecurityDeposit,
                LeaseTerms = lease.LeaseTerms,
                IsActive = lease.IsActive,
                CreatedAt = lease.CreatedAt,
                LastUpdated = lease.LastUpdated
            };
        }

        public async Task<LeaseDto> CreateLeaseAsync(CreateLeaseDto createLeaseDto, string userId, string role)
        {
            // Add authorization logic here
            if (role != "Admin" && role != "Landlord")
            {
                throw new UnauthorizedAccessException("You are not authorized to create a lease.");
            }

            var lease = new Lease
            {
                PropertyId = createLeaseDto.PropertyId,
                TenantId = createLeaseDto.TenantId,
                StartDate = createLeaseDto.StartDate,
                EndDate = createLeaseDto.EndDate,
                MonthlyRent = createLeaseDto.MonthlyRent,
                SecurityDeposit = createLeaseDto.SecurityDeposit,
                LeaseTerms = createLeaseDto.LeaseTerms,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();

            return await GetLeaseAsync(lease.Id, userId, role);
        }
    }
}
