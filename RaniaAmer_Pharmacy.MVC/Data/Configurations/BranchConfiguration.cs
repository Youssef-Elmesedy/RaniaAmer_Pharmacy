using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaniaAmer_Pharmacy.MVC.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(300);
        builder.Property(b => b.WorkingHours).IsRequired().HasMaxLength(200);
        builder.Property(b => b.DeliveryAreaText).HasMaxLength(200);
        builder.Property(b => b.MapEmbedUrl).HasMaxLength(1000);
        builder.Property(b => b.MapDirectionsUrl).HasMaxLength(1000);
        builder.Property(b => b.DisplayOrder).IsRequired();

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt);
    }
}
