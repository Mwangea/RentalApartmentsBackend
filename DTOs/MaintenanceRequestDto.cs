using System.ComponentModel.DataAnnotations;

namespace RentalAppartments.DTOs
{
    public class MaintenanceRequestDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }
        public decimal? EstimatedCost { get; set; }
        public bool IsUrgent { get; set; }
    }
}
