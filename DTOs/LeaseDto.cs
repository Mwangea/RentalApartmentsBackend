using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class LeaseDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string LandlordId { get; set; }
        public string LandlordName { get; set; }
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string LeaseTerms { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class CreateLeaseDto
    {
        [Required]
        public int PropertyId { get; set; }
        [Required]
        public string TenantId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string LeaseTerms { get; set; }
    }

    public class UpdateLeaseDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MonthlyRent { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public string LeaseTerms { get; set; }
    }
}
