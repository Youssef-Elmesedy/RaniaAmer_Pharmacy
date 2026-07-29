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

        title = title.Trim();
        description = description?.Trim();

        var changed = false;

        if (Title != title)
        {
            Title = title;
            changed = true;
        }

        if (Description != description)
        {
            Description = description;
            changed = true;
        }

        if (changed)
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public OfferItem AddItem(Guid productId, decimal specialPrice)
    {
        if (productId == Guid.Empty)
            throw new BusinessException("المنتج مطلوب", nameof(productId));

        if (specialPrice <= 0)
            throw new BusinessException("سعر العرض يجب أن يكون أكبر من صفر", nameof(specialPrice));

        if (_items.Any(i => i.ProductId == productId))
            throw new BusinessException("هذا المنتج مضاف بالفعل في العرض", nameof(productId));

        var item = OfferItem.Create(this, productId, specialPrice);
        _items.Add(item);

        UpdatedAt = DateTime.UtcNow;
        return item;
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

    // Wipes the current bundle items and rebuilds it from scratch (used when editing an offer).
    // Returns the newly-created items: because their Id is a client-generated (already non-empty)
    // Guid, EF Core can't reliably tell them apart from a pre-existing row once they're only
    // discovered via collection fixup on an already-tracked Offer — the caller (repository) must
    // explicitly mark these as Added, or EF may issue an UPDATE for a row that doesn't exist yet.
    public IReadOnlyList<OfferItem> ReplaceItems(IEnumerable<(Guid ProductId, decimal SpecialPrice)> items)
    {
        var incoming = items.ToDictionary(x => x.ProductId);
        var newlyAddedItems = new List<OfferItem>();

        // حذف العناصر غير الموجودة
        foreach (var existing in _items.ToList())
        {
            if (!incoming.ContainsKey(existing.ProductId))
                _items.Remove(existing);
        }

        // إضافة أو تحديث
        foreach (var item in incoming)
        {
            var existing = _items.FirstOrDefault(x => x.ProductId == item.Key);

            if (existing == null)
            {
                newlyAddedItems.Add(AddItem(item.Key, item.Value.SpecialPrice));
            }
            else if (existing.SpecialPrice != item.Value.SpecialPrice)
            {
                existing.UpdatePrice(item.Value.SpecialPrice);
            }
        }

        UpdatedAt = DateTime.UtcNow;
        return newlyAddedItems;
    }

    private static void Validate(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("عنوان العرض مطلوب", nameof(title));

        if (title.Length > 100)
            throw new BusinessException("عنوان العرض لا يجب أن يتجاوز 100 حرف", nameof(title));

        if (description != null && description.Length > 300)
            throw new BusinessException("وصف العرض لا يجب أن يتجاوز 300 حرف", nameof(description));
    }
}
