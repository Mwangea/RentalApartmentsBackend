using RentalAppartments.DTOs;
using RentalAppartments.Models;
using System.Threading.Tasks;

namespace RentalAppartments.Interfaces
{
    public interface IPaymentService
    {
        //Task<PaymentResult> InitiateMpesaPaymentAsync(MpesaPaymentRequest request, string userId);
        Task<PaymentResult> InitiatePayment(MpesaPaymentRequest paymentRequest);
        Task<PaymentResult> InitiateCashPayment(CashPaymentRequest request);
        Task<IEnumerable<object>> GetSuccessfulPaymentsForAdmin();
        Task<IEnumerable<object>> GetSuccessfulPaymentsForLandlord(string landlordId, int propertyId);

        Task<IEnumerable<Payment>> GetPaymentsForTenantAsync(string tenantId);

        Task<bool> HandleMpesaCallbackAsync(MpesaCallbackDto callbackData);
        Task<Payment> GetPaymentByTransactionIdAsync(string transactionId);
    }
}
