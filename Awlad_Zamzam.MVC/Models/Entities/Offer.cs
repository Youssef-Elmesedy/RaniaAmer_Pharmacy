using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

// A promotional bundle offer that groups several products together
public class Offer : BaseEntity
{
    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    private readonly List<OfferItem> _items = new();
    public IReadOnlyCollection<OfferItem> Items => _items.AsReadOnly();

    private Offer()
    {
    }

    public static Offer Create(string title, string? description)
    {
        Validate(title, description);

        return new Offer
        {
            Title = title.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description)
    {
        Validate(title, description);

        Title = title.Trim();
        Description = description?.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, decimal specialPrice)
    {
        if (productId == Guid.Empty)
            throw new BusinessException("Product is required.", nameof(productId));

        if (specialPrice <= 0)
            throw new BusinessException("Special price must be greater than zero.", nameof(specialPrice));

        if (_items.Any(i => i.ProductId == productId))
            throw new BusinessException("This product is already part of the offer.", nameof(productId));

        _items.Add(OfferItem.Create(Id, productId, specialPrice));

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid offerItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == offerItemId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Wipes the current bundle items and rebuilds it from scratch (used when editing an offer)
    public void ReplaceItems(IEnumerable<(Guid ProductId, decimal SpecialPrice)> items)
    {
        _items.Clear();

        foreach (var (productId, specialPrice) in items)
            AddItem(productId, specialPrice);

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("Offer title cannot be empty.", nameof(title));

        if (title.Length > 100)
            throw new BusinessException("Offer title cannot exceed 100 characters.", nameof(title));

        if (description != null && description.Length > 300)
            throw new BusinessException("Offer description cannot exceed 300 characters.", nameof(description));
    }
}
