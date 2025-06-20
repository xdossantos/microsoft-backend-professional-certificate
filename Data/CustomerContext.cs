using Microsoft.EntityFrameworkCore;
using AcmeDealership.Models;

namespace AcmeDealership.Data
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
        {
        }
        
        public DbSet<Customer> Customers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Age).IsRequired();
                entity.Property(e => e.AcceptedOffers).IsRequired();
                entity.Property(e => e.CanceledOffers).IsRequired();
                entity.Property(e => e.AverageReplyTime).IsRequired();
                
                // Configure Location as owned entity (embedded)
                entity.OwnsOne(e => e.Location, location =>
                {
                    location.Property(l => l.Latitude).IsRequired();
                    location.Property(l => l.Longitude).IsRequired();
                });
                
                // Add indexes for better query performance
                entity.HasIndex(e => e.Age);
                entity.HasIndex(e => e.AcceptedOffers);
                entity.HasIndex(e => e.CanceledOffers);
                entity.HasIndex(e => e.AverageReplyTime);
            });
        }
    }
}
