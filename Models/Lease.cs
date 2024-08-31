using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.Models
{
    public class Lease
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        [Required]
        public string TenantId { get; set; }
        public User Tenant { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal MonthlyRent { get; set; }

        public decimal SecurityDeposit { get; set; }

        public string LeaseTerms { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Navigation properties
        public ICollection<Payment> Payments { get; set; }
    }
}
