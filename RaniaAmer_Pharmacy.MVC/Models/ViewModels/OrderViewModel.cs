using RaniaAmer_Pharmacy.MVC.Models.Enums;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class AdminOrderListViewModel
{
    public List<OrderListItemViewModel> Orders { get; set; } = new();

    public string? SearchTerm { get; set; }
    public string SortOrder { get; set; } = "newest";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class OrderListItemViewModel
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public bool IsCredit { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public int ItemsCount { get; set; }

    public decimal RemainingBalance => Total - AmountPaid;
    public bool IsFullyPaid => RemainingBalance <= 0;
}

public class OrderDetailsViewModel
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public bool IsCredit { get; set; }
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
    public List<OrderPaymentViewModel> Payments { get; set; } = new();

    public decimal Total => Items.Sum(i => i.LineTotal);
    public decimal AmountPaid => Payments.Sum(p => p.Amount);
    public decimal RemainingBalance => Total - AmountPaid;
    public bool IsFullyPaid => RemainingBalance <= 0;
}

public class OrderItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string SaleUnitName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Note { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public string UnitLabel => SaleUnitName;
}

public class OrderPaymentViewModel
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime PaidAt { get; set; }
}
