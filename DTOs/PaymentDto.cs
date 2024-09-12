
namespace RentalAppartments.DTOs
{
    public class PaymentDto
    {

    }

    public class MpesaPaymentRequestDto
    {
        public int LeaseId { get; set; }
        public int PropertyId { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class MpesaPaymentRequest
    {
        public int LeaseId { get; set; }
        public int PropertyId { get; set; }
        public string TenantId { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
    }
}
