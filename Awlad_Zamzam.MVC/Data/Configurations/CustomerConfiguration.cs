using Awlad_Zamzam.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Awlad_Zamzam.MVC.Data.Configurations;

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

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => c.NormalizedName);

        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();
    }
}