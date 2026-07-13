using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Order? Order { get; private set; }

    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    // Snapshot of the product name/price at the time of the order,
    // so historical orders stay accurate even if the product changes later
    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public string? Note { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem()
    {
    }

    internal static OrderItem Create(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity, string? note)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new BusinessException("Product name cannot be empty.", nameof(productName));

        if (unitPrice < 0)
            throw new BusinessException("Unit price cannot be negative.", nameof(unitPrice));

        if (quantity <= 0)
            throw new BusinessException("Quantity must be greater than zero.", nameof(quantity));

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
            Note = note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
