using Microsoft.EntityFrameworkCore;
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

        public MaintenanceService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

        public async Task<MaintenanceRequest> CreateMaintenanceRequestAsync(string tenantId, MaintenanceRequestDto requestDto)
        {
            var maintenanceRequest = new MaintenanceRequest
            {
                PropertyId = requestDto.PropertyId,
                TenantId = tenantId,
                Title = requestDto.Title,
                Description = requestDto.Description,
                Status = requestDto.Status,
                CreatedAt = requestDto.CreatedAt,
                Notes = requestDto.Notes,
                Cost = requestDto.EstimatedCost
            };

            _context.MaintenanceRequests.Add(maintenanceRequest);
            await _context.SaveChangesAsync();

            // Notify landlord about new maintenance request
            var property = await _context.Properties.FindAsync(requestDto.PropertyId);
            if (property != null)
            {
                await _notificationService.CreateNotificationAsync(new MaintenanceUpdateDto
                {
                    UserId = property.LandlordId,
                    Title = "New Maintenance Request",
                    Message = $"A new maintenance request has been created for property {property.Address}",
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