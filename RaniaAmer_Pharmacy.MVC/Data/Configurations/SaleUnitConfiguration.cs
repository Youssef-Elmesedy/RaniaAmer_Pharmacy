using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaniaAmer_Pharmacy.MVC.Data.Configurations;

public class SaleUnitConfiguration : IEntityTypeConfiguration<SaleUnit>
{
    public void Configure(EntityTypeBuilder<SaleUnit> builder)
    {
        builder.ToTable("SaleUnits");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.NormalizedName)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasIndex(u => u.NormalizedName)
            .IsUnique();

        builder.HasMany(u => u.Products)
            .WithOne(p => p.SaleUnit)
            .HasForeignKey(p => p.SaleUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
