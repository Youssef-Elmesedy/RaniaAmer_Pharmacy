using Awlad_Zamzam.MVC.Models.Enums;
using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

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

    // Snapshot of how the product was sold at order time (per piece or per kilogram)
    public SaleUnit SaleUnit { get; private set; } = SaleUnit.Piece;

    // A whole number of items when SaleUnit is Piece, or a weight in kg (can be fractional,
    // e.g. 1.5) when SaleUnit is Kilogram
    public decimal Quantity { get; private set; }

    public string? Note { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem()
    {
    }

    internal static OrderItem Create(Guid orderId, Guid productId, string productName, decimal unitPrice, SaleUnit saleUnit, decimal quantity, string? note)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new BusinessException("اسم المنتج مطلوب", nameof(productName));

        if (unitPrice < 0)
            throw new BusinessException("السعر لا يمكن أن يكون سالبًا", nameof(unitPrice));

        if (quantity <= 0)
            throw new BusinessException("الكمية يجب أن تكون أكبر من صفر", nameof(quantity));

        if (saleUnit == SaleUnit.Piece && quantity != Math.Floor(quantity))
            throw new BusinessException("الكمية يجب أن تكون رقم صحيح للمنتجات المباعة بالقطعة", nameof(quantity));

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            SaleUnit = saleUnit,
            Quantity = quantity,
            Note = note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
