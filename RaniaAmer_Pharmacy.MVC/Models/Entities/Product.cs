using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    // Price is charged per the selected sale unit (e.g. per box, per strip, per bottle, etc.)
    public Guid SaleUnitId { get; private set; }

    public SaleUnit? SaleUnit { get; private set; }

    public int DiscountPercentage { get; private set; }

    public string? ImagePath { get; private set; }

    public bool IsAvailable { get; private set; } = true;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    private readonly List<ProductUnitOption> _unitOptions = new();
    public IReadOnlyCollection<ProductUnitOption> UnitOptions => _unitOptions.AsReadOnly();

    private Product()
    {
    }

    public static Product Create(string name, string description, decimal price, Guid saleUnitId, int discountPercentage, string? imagePath, Guid categoryId)
    {
        Validate(name, description, price, discountPercentage, categoryId, saleUnitId);

        return new Product
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            Price = price,
            SaleUnitId = saleUnitId,
            DiscountPercentage = discountPercentage,
            ImagePath = imagePath,
            CategoryId = categoryId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, decimal price, Guid saleUnitId, int discountPercentage, string? imagePath, Guid categoryId)
    {
        Validate(name, description, price, discountPercentage, categoryId, saleUnitId);

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description.Trim();
        Price = price;
        SaleUnitId = saleUnitId;
        DiscountPercentage = discountPercentage;
        ImagePath = imagePath;
        CategoryId = categoryId;

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsAvailable()
    {
        IsAvailable = true;

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsUnavailable()
    {
        IsAvailable = false;

        UpdatedAt = DateTime.UtcNow;
    }

    // Wipes the current sub-unit options and rebuilds them from scratch (used when editing a
    // product). Returns the newly-created options: the repository must explicitly mark these
    // as Added — same EF tracking caveat as Offer.ReplaceItems.
    public IReadOnlyList<ProductUnitOption> ReplaceUnitOptions(IEnumerable<(Guid SaleUnitId, int QuantityPerBaseUnit)> options)
    {
        var incoming = options
            .Where(o => o.SaleUnitId != SaleUnitId) // a sub-unit can't be the same as the base unit
            .ToDictionary(o => o.SaleUnitId);

        var newlyAddedOptions = new List<ProductUnitOption>();

        foreach (var existing in _unitOptions.ToList())
        {
            if (!incoming.ContainsKey(existing.SaleUnitId))
                _unitOptions.Remove(existing);
        }

        foreach (var option in incoming)
        {
            var existing = _unitOptions.FirstOrDefault(o => o.SaleUnitId == option.Key);

            if (existing == null)
            {
                var newOption = ProductUnitOption.Create(Id, option.Key, option.Value.QuantityPerBaseUnit);
                _unitOptions.Add(newOption);
                newlyAddedOptions.Add(newOption);
            }
            else
            {
                existing.UpdateQuantity(option.Value.QuantityPerBaseUnit);
            }
        }

        UpdatedAt = DateTime.UtcNow;
        return newlyAddedOptions;
    }

    private static void Validate(string name, string description, decimal price, int discountPercentage, Guid categoryId, Guid saleUnitId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("اسم المنتج مطلوب", nameof(name));

        if (name.Length > 50)
            throw new BusinessException("اسم المنتج لا يجب أن يتجاوز 50 حرف", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("الوصف مطلوب", nameof(description));

        if (description.Length > 100)
            throw new BusinessException("الوصف لا يجب أن يتجاوز 100 حرف", nameof(description));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new BusinessException("نسبة الخصم يجب أن تكون بين 0 و 100", nameof(discountPercentage));

        if (price <= 0)
            throw new BusinessException("السعر يجب أن يكون أكبر من صفر", nameof(price));

        if (categoryId == Guid.Empty)
            throw new BusinessException("القسم مطلوب", nameof(categoryId));

        if (saleUnitId == Guid.Empty)
            throw new BusinessException("وحدة البيع مطلوبة", nameof(saleUnitId));
    }
}
