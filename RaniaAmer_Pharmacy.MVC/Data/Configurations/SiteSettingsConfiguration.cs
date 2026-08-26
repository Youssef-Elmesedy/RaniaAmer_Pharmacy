using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RaniaAmer_Pharmacy.MVC.Data.Configurations;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.PharmacyName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.WhatsAppNumber).HasMaxLength(30);
        builder.Property(s => s.FacebookUrl).HasMaxLength(300);
        builder.Property(s => s.InstagramUrl).HasMaxLength(300);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);
    }
}
