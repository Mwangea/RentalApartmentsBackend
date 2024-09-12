using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RentalAppartments.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        public string LandlordId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal RentAmount { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public decimal SquareFootage { get; set; }

        public bool IsAvailable { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdated { get; set; }

        public string? CurrentTenantId { get; set; }

        public string ImageUrls { get; set; } // Store as JSON string

        // Navigation properties
        public User Landlord { get; set; }
        public User? CurrentTenant { get; set; }
        public ICollection<Lease> Leases { get; set; }

        [JsonIgnore]
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; }
        public ICollection<Payment> Payments { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }
    }
}