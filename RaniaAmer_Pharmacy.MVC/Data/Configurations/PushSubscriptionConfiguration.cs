using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaniaAmer_Pharmacy.MVC.Data.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Endpoint)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.P256dh)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Auth)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.AdminUserId)
            .HasMaxLength(450); // matches IdentityUser.Id max length

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Re-subscribing on the same browser should update, not duplicate, the row
        builder.HasIndex(p => p.Endpoint)
            .IsUnique();

        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.AdminUserId);

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("PushSubscriptions");
    }
}
