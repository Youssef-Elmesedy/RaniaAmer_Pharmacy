using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaniaAmer_Pharmacy.MVC.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.NormalizedName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.PasswordHash)
            .HasMaxLength(300);

        builder.Property(c => c.SecurityQuestion)
            .HasMaxLength(200);

        builder.Property(c => c.SecurityAnswerHash)
            .HasMaxLength(300);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.LastActivityAt);

        builder.Property(c => c.DeactivatedAt);

        builder.HasIndex(c => c.NormalizedName);

        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();

        // Supports the inactive-customer cleanup scan (IsActive customers ordered by activity)
        builder.HasIndex(c => new { c.IsActive, c.LastActivityAt });
    }
}