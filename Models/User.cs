using Microsoft.AspNetCore.Identity;
using RentalAppartments.Models;
using RRentalAppartments.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RentalAppartments.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public string Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }

        public virtual ICollection<Lease> Leases { get; set; }

        [JsonIgnore]
        public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }

        public bool EmailNotifications { get; set; }

        public bool SmsNotifications { get; set; }

        public bool PushNotifications { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }

        // New properties
        public virtual ICollection<Property> Properties { get; set; }
        public virtual ICollection<Property> RentedProperties { get; set; }
    }
}