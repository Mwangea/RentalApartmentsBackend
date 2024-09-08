using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using RentalApartmentSystem.API.Controllers;
using RentalAppartments.Data;
using RentalAppartments.DTOs;
using RentalAppartments.Interfaces;
using RentalAppartments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalAppartments.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MaintenanceService> _logger;
        private readonly IPropertyService _propertyService;

        public MaintenanceService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<MaintenanceService> logger,
            IPropertyService propertyService)

        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetAllMaintenanceRequestsAsync()
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetMaintenanceRequestsByTenantIdAsync(string tenantId)
        {
            return await _context.MaintenanceRequests
                .Where(m => m.TenantId == tenantId)
                .Include(m => m.Property)
                .ToListAsync();
        }

        public async Task<MaintenanceRequest> GetMaintenanceRequestByIdAsync(int id)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MaintenanceRequest> CreateMaintenanceRequestAsync(MaintenanceRequestDto requestDto)
        {
            _logger.LogInformation($"Attempting to create maintenance request for tenant ID: {requestDto.TenantId}");

            var maintenanceRequest = new MaintenanceRequest
            {
                PropertyId = requestDto.PropertyId,
                TenantId = requestDto.TenantId,
                Title = requestDto.Title,
                Description = requestDto.Description,
                Status = requestDto.Status ?? "Pending",
                CreatedAt = DateTime.UtcNow,
                Notes = requestDto.Notes,
                Cost = requestDto.EstimatedCost,
                IsUrgent = requestDto.IsUrgent ?? false
            };

            _context.MaintenanceRequests.Add(maintenanceRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving maintenance request");
                throw new InvalidOperationException("Unable to create maintenance request. There was an error saving to the database.", ex);
            }

            // Notify landlord about new maintenance request
            var property = await _context.Properties.FindAsync(requestDto.PropertyId);
            if (property != null)
            {
                await _notificationService.CreateNotificationAsync(new MaintenanceUpdateDto
                {
                    UserId = property.LandlordId,
                    Title = "New Maintenance Request",
                    Message = $"A new maintenance request has been created for property {requestDto.PropertyName} by tenant {requestDto.TenantName}",
                    Status = "New"
                });
            }

            return maintenanceRequest;
        }

        public async Task<MaintenanceRequest> UpdateMaintenanceRequestAsync(int id, MaintenanceRequestDto requestDto)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null)
                return null;

            maintenanceRequest.Title = requestDto.Title;
            maintenanceRequest.Description = requestDto.Description;
            maintenanceRequest.Status = requestDto.Status;
            maintenanceRequest.Notes = requestDto.Notes;
            maintenanceRequest.Cost = requestDto.EstimatedCost;
            maintenanceRequest.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return maintenanceRequest;
        }

        public async Task<MaintenanceRequest> UpdateMaintenanceRequestStatusAsync(int id, string status)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null)
                return null;

            maintenanceRequest.Status = status;
            maintenanceRequest.LastUpdated = DateTime.UtcNow;

            if (status == "Completed")
                maintenanceRequest.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify tenant about status update
            await _notificationService.CreateNotificationAsync(new MaintenanceUpdateDto
            {
                UserId = maintenanceRequest.TenantId,
                Title = "Maintenance Request Update",
                Message = $"Your maintenance request '{maintenanceRequest.Title}' status has been updated to {status}",
                Status = status
            });

            return maintenanceRequest;
        }

        public async Task<bool> DeleteMaintenanceRequestAsync(int id)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null)
                return false;

            _context.MaintenanceRequests.Remove(maintenanceRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SendMaintenanceUpdateAsync(int id, MaintenanceUpdateDto updateDto)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null)
                return false;

            // Update the maintenance request
            maintenanceRequest.Status = updateDto.Status;
            maintenanceRequest.LastUpdated = DateTime.UtcNow;
            maintenanceRequest.Notes += $"\n{DateTime.UtcNow}: {updateDto.Message}";

            if (updateDto.ScheduledDate.HasValue)
                maintenanceRequest.Notes += $"\nScheduled for: {updateDto.ScheduledDate.Value:g}";

            await _context.SaveChangesAsync();

            // Send notification to tenant
            await _notificationService.CreateNotificationAsync(new MaintenanceUpdateDto
            {
                UserId = maintenanceRequest.TenantId,
                Title = updateDto.Title,
                Message = updateDto.Message,
                Status = updateDto.Status,
                ScheduledDate = updateDto.ScheduledDate
            });

            return true;
        }
    }
}