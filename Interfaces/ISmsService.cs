using RRentalAppartments.Models;

namespace RentalAppartments.Interfaces
{
    public interface ISmsService
    {
        Task SendRentReminderSms(Notification notification);
        Task SendMaintenanceUpdateSms(Notification notification);
        Task SendTestSms(string phoneNumber, string message);
    }
}
