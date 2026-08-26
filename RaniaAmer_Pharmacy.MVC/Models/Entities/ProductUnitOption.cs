using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

// A secondary way to sell a product, expressed as how many of this unit fit inside ONE of
// the product's base/purchasing unit (e.g. Product sold in "علبة" at 60 ج.م، and a
// ProductUnitOption of "شريط" with QuantityPerBaseUnit = 2 means the box contains 2 strips,
// so each strip costs 30 ج.م — computed on the fly, never stored.
public class ProductUnitOption : BaseEntity
{
    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    public Guid SaleUnitId { get; private set; }

    public SaleUnit? SaleUnit { get; private set; }

    // How many of this (smaller) unit make up one of the product's base unit.
    // e.g. base unit = علبة, this unit = قرص, QuantityPerBaseUnit = 20
    public int QuantityPerBaseUnit { get; private set; }

    private ProductUnitOption()
    {
    }

    internal static ProductUnitOption Create(Guid productId, Guid saleUnitId, int quantityPerBaseUnit)
    {
        if (quantityPerBaseUnit <= 1)
            throw new BusinessException("عدد الوحدات الفرعية يجب أن يكون أكبر من واحد", nameof(quantityPerBaseUnit));

        return new ProductUnitOption
        {
            ProductId = productId,
            SaleUnitId = saleUnitId,
            QuantityPerBaseUnit = quantityPerBaseUnit,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void UpdateQuantity(int quantityPerBaseUnit)
    {
        if (quantityPerBaseUnit <= 1)
            throw new BusinessException("عدد الوحدات الفرعية يجب أن يكون أكبر من واحد", nameof(quantityPerBaseUnit));

        if (QuantityPerBaseUnit == quantityPerBaseUnit)
            return;

        QuantityPerBaseUnit = quantityPerBaseUnit;
        UpdatedAt = DateTime.UtcNow;
    }
}
