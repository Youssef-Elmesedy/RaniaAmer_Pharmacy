using Awlad_Zamzam.MVC.Models.Enums;
using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    // Price is per-Kilogram when SaleUnit is Kilogram (e.g. fresh meat), or per-Piece otherwise
    public SaleUnit SaleUnit { get; private set; } = SaleUnit.Piece;

    public int DiscountPercentage { get; private set; }

    public string? ImagePath { get; private set; }

    public bool IsAvailable { get; private set; } = true;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    private Product()
    {
    }

    public static Product Create(string name, string description, decimal price, SaleUnit saleUnit, int discountPercentage, string? imagePath, Guid categoryId)
    {
        Validate(name, description, price, discountPercentage, categoryId);

        return new Product
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            Price = price,
            SaleUnit = saleUnit,
            DiscountPercentage = discountPercentage,
            ImagePath = imagePath,
            CategoryId = categoryId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, decimal price, SaleUnit saleUnit, int discountPercentage, string? imagePath, Guid categoryId)
    {
        Validate(name, description, price, discountPercentage, categoryId);

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description.Trim();
        Price = price;
        SaleUnit = saleUnit;
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

    private static void Validate(string name, string description, decimal price, int discountPercentage, Guid categoryId)
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
    }
}
