using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class MpesaPaymentRequest
    {
        [Required]
        public int LeaseId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        // Remove any validation attribute from TenantId
        public string TenantId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression(@"^254\d{9}$", ErrorMessage = "Phone number must be in the format 254XXXXXXXXX")]
        public string PhoneNumber { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public bool Cancelled { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
    }

    public class MpesaCallbackDto
    {
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public decimal Amount { get; set; }
        public string MpesaReceiptNumber { get; set; }
        public string PhoneNumber { get; set; }
    }
}