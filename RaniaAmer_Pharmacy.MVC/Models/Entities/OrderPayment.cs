using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

// A partial (or full) payment recorded by the admin against a credit ("آجل") order
public class OrderPayment : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Order? Order { get; private set; }

    public decimal Amount { get; private set; }

    public string? Notes { get; private set; }

    public DateTime PaidAt { get; private set; }

    private OrderPayment()
    {
    }

    internal static OrderPayment Create(Guid orderId, decimal amount, string? notes)
    {
        if (amount <= 0)
            throw new BusinessException("Payment amount must be greater than zero.", nameof(amount));

        return new OrderPayment
        {
            OrderId = orderId,
            Amount = amount,
            Notes = notes?.Trim(),
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
