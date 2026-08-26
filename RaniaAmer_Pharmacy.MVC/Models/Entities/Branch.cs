using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

// A physical branch of the pharmacy. The site can have one or many branches — each with its
// own address, phone, working hours and map — all shown on the same public site (footer shows
// the first branch, the contact page lists all of them).
public class Branch : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string WorkingHours { get; private set; } = string.Empty;

    public string? DeliveryAreaText { get; private set; }

    public string? MapEmbedUrl { get; private set; }

    public string? MapDirectionsUrl { get; private set; }

    // Controls display order on the contact page (and which branch shows in the footer — the
    // lowest number first). Doesn't need to be unique/contiguous.
    public int DisplayOrder { get; private set; }

    private Branch()
    {
    }

    public static Branch Create(string name, string phoneNumber, string address, string workingHours,
        string? deliveryAreaText, string? mapEmbedUrl, string? mapDirectionsUrl, int displayOrder)
    {
        Validate(name, phoneNumber, address, workingHours);

        return new Branch
        {
            Name = name.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Address = address.Trim(),
            WorkingHours = workingHours.Trim(),
            DeliveryAreaText = string.IsNullOrWhiteSpace(deliveryAreaText) ? null : deliveryAreaText.Trim(),
            MapEmbedUrl = string.IsNullOrWhiteSpace(mapEmbedUrl) ? null : mapEmbedUrl.Trim(),
            MapDirectionsUrl = string.IsNullOrWhiteSpace(mapDirectionsUrl) ? null : mapDirectionsUrl.Trim(),
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string phoneNumber, string address, string workingHours,
        string? deliveryAreaText, string? mapEmbedUrl, string? mapDirectionsUrl, int displayOrder)
    {
        Validate(name, phoneNumber, address, workingHours);

        Name = name.Trim();
        PhoneNumber = phoneNumber.Trim();
        Address = address.Trim();
        WorkingHours = workingHours.Trim();
        DeliveryAreaText = string.IsNullOrWhiteSpace(deliveryAreaText) ? null : deliveryAreaText.Trim();
        MapEmbedUrl = string.IsNullOrWhiteSpace(mapEmbedUrl) ? null : mapEmbedUrl.Trim();
        MapDirectionsUrl = string.IsNullOrWhiteSpace(mapDirectionsUrl) ? null : mapDirectionsUrl.Trim();
        DisplayOrder = displayOrder;

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string phoneNumber, string address, string workingHours)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("اسم الفرع مطلوب", nameof(name));

        if (name.Length > 100)
            throw new BusinessException("اسم الفرع لا يجب أن يتجاوز 100 حرف", nameof(name));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessException("رقم هاتف الفرع مطلوب", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessException("عنوان الفرع مطلوب", nameof(address));

        if (string.IsNullOrWhiteSpace(workingHours))
            throw new BusinessException("مواعيد عمل الفرع مطلوبة", nameof(workingHours));
    }
}
