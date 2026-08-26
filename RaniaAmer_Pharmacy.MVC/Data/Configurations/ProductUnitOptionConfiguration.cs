using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaniaAmer_Pharmacy.MVC.Data.Configurations;

public class ProductUnitOptionConfiguration : IEntityTypeConfiguration<ProductUnitOption>
{
    public void Configure(EntityTypeBuilder<ProductUnitOption> builder)
    {
        builder.ToTable("ProductUnitOptions");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.QuantityPerBaseUnit)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.HasIndex(o => new { o.ProductId, o.SaleUnitId })
            .IsUnique();

        builder.HasOne(o => o.SaleUnit)
            .WithMany()
            .HasForeignKey(o => o.SaleUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
