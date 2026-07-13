namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class PaymentPageViewModel
{
    public string? SearchTerm { get; set; }

    public List<CustomerSelectItem> Customers { get; set; } = new();

    public Guid? SelectedCustomerId { get; set; }
    public string? SelectedCustomerName { get; set; }
    public string? SelectedCustomerPhone { get; set; }

    public List<OrderListItemViewModel> CreditOrders { get; set; } = new();

    public decimal TotalDue => CreditOrders.Sum(o => o.RemainingBalance);
    public decimal TotalOrdersAmount => CreditOrders.Sum(o => o.Total);
    public decimal TotalPaidSoFar => CreditOrders.Sum(o => o.AmountPaid);
}

public class CustomerSelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal TotalCreditDue { get; set; }
}
