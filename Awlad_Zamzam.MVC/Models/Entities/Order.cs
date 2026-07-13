using Awlad_Zamzam.MVC.Models.Enums;
using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

// A customer order/cart submission. Can be a regular order or a credit ("آجل") order.
public class Order : BaseEntity
{
    public Guid CustomerId { get; private set; }

    public Customer? Customer { get; private set; }

    public bool IsCredit { get; private set; }

    public string? Notes { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public DateTime OrderDate { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderPayment> _payments = new();
    public IReadOnlyCollection<OrderPayment> Payments => _payments.AsReadOnly();

    public decimal Total => _items.Sum(i => i.LineTotal);

    public decimal AmountPaid => _payments.Sum(p => p.Amount);

    public decimal RemainingBalance => Total - AmountPaid;

    public bool IsFullyPaid => RemainingBalance <= 0;

    private Order()
    {
    }

    public static Order Create(Guid customerId, string? notes)
    {
        if (customerId == Guid.Empty)
            throw new BusinessException("Customer is required.", nameof(customerId));

        if (notes != null && notes.Length > 500)
            throw new BusinessException("Notes cannot exceed 500 characters.", nameof(notes));

        return new Order
        {
            CustomerId = customerId,
            IsCredit = false,
            Notes = notes?.Trim(),
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity, string? note)
    {
        _items.Add(OrderItem.Create(Id, productId, productName, unitPrice, quantity, note));
        UpdatedAt = DateTime.UtcNow;
    }

    // Only the admin decides, at delivery time, whether the order was paid in cash or taken on credit ("آجل")
    public void Complete(bool isCredit)
    {
        Status = OrderStatus.Completed;
        IsCredit = isCredit;
        UpdatedAt = DateTime.UtcNow;
    }

    // Pure validation for recording a payment; the OrderPayment itself is created and persisted
    // independently by the service layer (see OrderService.AddPaymentAsync) to keep the write
    // isolated from this aggregate's change-tracking.
    public void EnsureCanAcceptPayment(decimal amount)
    {
        if (!IsCredit)
            throw new BusinessException("لا يمكن تسجيل دفعات إلا على طلبات الآجل.", nameof(amount));

        if (IsFullyPaid)
            throw new BusinessException("تم سداد هذا الطلب بالكامل بالفعل.", nameof(amount));

        if (amount <= 0)
            throw new BusinessException("المبلغ يجب أن يكون أكبر من صفر.", nameof(amount));

        if (amount > RemainingBalance)
            throw new BusinessException($"المبلغ أكبر من المتبقي ({RemainingBalance:0.00} ج.م).", nameof(amount));
    }
}
