using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class PropertyDto
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

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdated { get; set; }

        public List<string> ImageUrls { get; set; }

        public string? CurrentTenantId { get; set; }
    }
}