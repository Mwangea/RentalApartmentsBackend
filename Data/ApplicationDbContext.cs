using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalAppartments.Models;
using RRentalAppartments.Models;

namespace RentalAppartments.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasOne(p => p.Landlord)
                    .WithMany(u => u.Properties)
                    .HasForeignKey(p => p.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.CurrentTenant)
                    .WithMany(u => u.RentedProperties)
                    .HasForeignKey(p => p.CurrentTenantId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(p => p.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Available");

                entity.Property(p => p.IsAvailable)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<Lease>(entity =>
            {
                entity.HasOne(l => l.Tenant)
                    .WithMany(u => u.Leases)
                    .HasForeignKey(l => l.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Property)
                    .WithMany(p => p.Leases)
                    .HasForeignKey(l => l.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaintenanceRequest>(entity =>
            {
                entity.HasOne(m => m.Tenant)
                    .WithMany(u => u.MaintenanceRequests)
                    .HasForeignKey(m => m.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Property)
                    .WithMany(p => p.MaintenanceRequests)
                    .HasForeignKey(m => m.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.Tenant)
                    .WithMany(u => u.Payments)
                    .HasForeignKey(p => p.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Property)
                    .WithMany(prop => prop.Payments)
                    .HasForeignKey(p => p.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}