using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

// Singleton settings row (there is always exactly one) holding the business-wide info that's
// shared across every branch: name, WhatsApp, and social links. Per-branch info (address,
// phone, hours, map) lives in the Branch entity instead, since a business can have more than
// one physical branch on the same site.
public class SiteSettings : BaseEntity
{
    public string PharmacyName { get; private set; } = string.Empty;

    public string? WhatsAppNumber { get; private set; }

    public string? FacebookUrl { get; private set; }

    public string? InstagramUrl { get; private set; }

    private SiteSettings()
    {
    }

    public static SiteSettings CreateDefault() => new()
    {
        PharmacyName = "صيدلية رانيا عامر",
        CreatedAt = DateTime.UtcNow
    };

    public void Update(
        string pharmacyName,
        string? whatsAppNumber,
        string? facebookUrl,
        string? instagramUrl)
    {
        if (string.IsNullOrWhiteSpace(pharmacyName))
            throw new BusinessException("اسم الصيدلية مطلوب", nameof(pharmacyName));

        PharmacyName = pharmacyName.Trim();
        WhatsAppNumber = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : whatsAppNumber.Trim();
        FacebookUrl = string.IsNullOrWhiteSpace(facebookUrl) ? null : facebookUrl.Trim();
        InstagramUrl = string.IsNullOrWhiteSpace(instagramUrl) ? null : instagramUrl.Trim();

        UpdatedAt = DateTime.UtcNow;
    }
}
