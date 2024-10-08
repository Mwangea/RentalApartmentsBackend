﻿using RentalAppartments.DTOs;
using RentalAppartments.Models;

namespace RentalAppartments.Interfaces
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceRequest>> GetAllMaintenanceRequestsAsync();
        Task<IEnumerable<MaintenanceRequest>> GetMaintenanceRequestsByTenantIdAsync(string tenantId);
        Task<MaintenanceRequest> GetMaintenanceRequestByIdAsync(int id);
        Task<MaintenanceRequest> CreateMaintenanceRequestAsync(MaintenanceRequestDto requestDto);
        Task<MaintenanceRequest> UpdateMaintenanceRequestAsync(int id, MaintenanceRequest request);
        Task<MaintenanceRequest> UpdateMaintenanceRequestStatusAsync(int id, string status);
        Task<bool> DeleteMaintenanceRequestAsync(int id);
        Task<bool> SendMaintenanceUpdateAsync(int id, MaintenanceUpdateDto updateDto);
    }   

}
