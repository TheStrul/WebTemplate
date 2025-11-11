using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTemplate.Core.Entities;

namespace WebTemplate.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.PhoneNumber2).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                
                // Configure relationship with UserType
                entity.HasOne(u => u.UserType)
                      .WithMany(ut => ut.Users)
                      .HasForeignKey(u => u.UserTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Add indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserTypeId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure UserType
            builder.Entity<UserType>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Permissions).HasMaxLength(2000);
                
                // Add indexes
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.DeviceName).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                // Configure relationship with ApplicationUser
                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Add indexes for performance
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => e.DeviceId);
                entity.HasIndex(e => new { e.UserId, e.ExpiryDate });
                entity.HasIndex(e => new { e.UserId, e.RevokedAt });
            });

        }

    }
}
