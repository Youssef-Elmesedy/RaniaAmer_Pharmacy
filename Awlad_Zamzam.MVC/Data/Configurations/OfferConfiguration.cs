using Awlad_Zamzam.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Awlad_Zamzam.MVC.Data.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Description)
            .HasMaxLength(300);

        builder.Property(o => o.IsActive)
            .HasDefaultValue(true);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.Metadata.FindNavigation(nameof(Offer.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Offer)
            .HasForeignKey(i => i.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
       .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
               .FindNavigation(nameof(Offer.Items))!
               .SetField("_items");
    }
}
