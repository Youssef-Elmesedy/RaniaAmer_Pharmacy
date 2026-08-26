using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Order? Order { get; private set; }

    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    // Snapshot of the product name/price/unit at the time of the order,
    // so historical orders stay accurate even if the product changes later
    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    // Snapshot of the sale unit's display name at order time (e.g. "شريط"). Stored as text
    // rather than a foreign key so historical orders stay accurate even if the admin later
    // renames or deletes that sale unit.
    public string SaleUnitName { get; private set; } = string.Empty;

    // Always a whole number of units (pieces, boxes, strips, etc.)
    public decimal Quantity { get; private set; }

    public string? Note { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem()
    {
    }

    internal static OrderItem Create(Guid orderId, Guid productId, string productName, decimal unitPrice, string saleUnitName, decimal quantity, string? note)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new BusinessException("اسم المنتج مطلوب", nameof(productName));

        if (unitPrice < 0)
            throw new BusinessException("السعر لا يمكن أن يكون سالبًا", nameof(unitPrice));

        if (quantity <= 0)
            throw new BusinessException("الكمية يجب أن تكون أكبر من صفر", nameof(quantity));

        if (quantity != Math.Floor(quantity))
            throw new BusinessException("الكمية يجب أن تكون رقم صحيح", nameof(quantity));

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            SaleUnitName = string.IsNullOrWhiteSpace(saleUnitName) ? "قطعة" : saleUnitName.Trim(),
            Quantity = quantity,
            Note = note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
