using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class CashPaymentRequest : IPaymentRequest
    {
        [Required]
        public int LeaseId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string TenantId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}
