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
        private readonly ILogger<LeaseService> _logger;

        public LeaseService(ApplicationDbContext context, ILogger<LeaseService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LeaseDto> GetLeaseAsync(int id, string userId, string role)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                    .ThenInclude(p => p.Landlord)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null)
            {
                _logger.LogWarning("Lease {LeaseId} not found", id);
                return null;
            }

            if (role != "Admin" && lease.Tenant.Id != userId && lease.Property.LandlordId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt to lease {LeaseId} by user {UserId}", id, userId);
                throw new UnauthorizedAccessException("You are not authorized to view this lease.");
            }

            return MapLeaseToDto(lease);
        }

        public async Task<IEnumerable<LeaseDto>> GetAllLeasesAsync()
        {
            var leases = await _context.Leases
                .Include(l => l.Property)
                    .ThenInclude(p => p.Landlord)
                .Include(l => l.Tenant)
                .ToListAsync();

            return leases.Select(MapLeaseToDto);
        }

        public async Task<LeaseDto> CreateLeaseAsync(CreateLeaseDto createLeaseDto, string userId, string role)
        {
            if (role != "Admin" && role != "Landlord")
            {
                _logger.LogWarning("Unauthorized create lease attempt by user {UserId} with role {Role}", userId, role);
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

            _logger.LogInformation("Lease {LeaseId} created by user {UserId}", lease.Id, userId);

            return await GetLeaseAsync(lease.Id, userId, role);
        }

        public async Task<LeaseDto> UpdateLeaseAsync(int id, UpdateLeaseDto updateLeaseDto, string userId, string role)
        {
           // _logger.LogInformation("Attempting to update lease {LeaseId} by user {UserId} with role {Role}", id, userId, role);

            if (updateLeaseDto == null)
            {
                throw new ArgumentNullException(nameof(updateLeaseDto));
            }

            var lease = await _context.Leases
                .Include(l => l.Property)
                    .ThenInclude(p => p.Landlord)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null)
            {
               // _logger.LogWarning("Lease {LeaseId} not found", id);
                return null;
            }

            if (role != "Admin" && lease.Property.LandlordId != userId)
            {
               // _logger.LogWarning("Unauthorized update attempt to lease {LeaseId} by user {UserId}", id, userId);
                throw new UnauthorizedAccessException("You are not authorized to update this lease");
            }

            // Update lease properties
            lease.StartDate = updateLeaseDto.StartDate ?? lease.StartDate;
            lease.EndDate = updateLeaseDto.EndDate ?? lease.EndDate;
            lease.MonthlyRent = updateLeaseDto.MonthlyRent ?? lease.MonthlyRent;
            lease.SecurityDeposit = updateLeaseDto.SecurityDeposit ?? lease.SecurityDeposit;
            lease.LeaseTerms = updateLeaseDto.LeaseTerms ?? lease.LeaseTerms;
            lease.LastUpdated = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
               // _logger.LogInformation("Lease {LeaseId} updated successfully", id);
            }
            catch (DbUpdateException ex)
            {
               // _logger.LogError(ex, "Error occurred while saving updated lease {LeaseId} to database", id);
                throw;
            }

            return MapLeaseToDto(lease);
        }


        public async Task<bool> DeleteLeaseAsync(int id, string userId, string role)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null) return false;

            // Check if the user is authorized to delete the lease
            if (role != "Admin" && lease.Property?.LandlordId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this lease.");
            }

            _context.Leases.Remove(lease);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<IEnumerable<LeaseDto>> GetLeasesByPropertyAsync(int propertyId, string userId, string role)
        {
            var leases = await _context.Leases
                .Where(l => l.PropertyId == propertyId)
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
    }
}