namespace RentalAppartments.DTOs
{
    public interface IPaymentRequest
    {
        int LeaseId { get; set; }
        int PropertyId { get; set; }
        string TenantId { get; set; }
        decimal Amount { get; set; }
    }
}
