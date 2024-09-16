using RentalAppartments.DTOs;
using System.Threading.Tasks;

namespace RentalAppartments.Interfaces
{
    public interface IPaymentService
    {
        //Task<PaymentResult> InitiateMpesaPaymentAsync(MpesaPaymentRequest request, string userId);
        Task<PaymentResult> InitiatePayment(MpesaPaymentRequest paymentRequest);
    }
}
