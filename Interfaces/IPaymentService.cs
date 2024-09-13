using RentalAppartments.DTOs;
using RentalAppartments.Models;

namespace RentalAppartments.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessMpesaPaymentAsync(MpesaPaymentRequest request, string tenantId);
        //Task<Payment> GetPaymentByIdAsync(int paymentId);
        // Task<IEnumerable<Payment>> GetPaymentsByTenantIdAsync(string tenantId);
        // Task<IEnumerable<Payment>> GetPaymentsByPropertyIdAsync(int propertyId);
    }
}
