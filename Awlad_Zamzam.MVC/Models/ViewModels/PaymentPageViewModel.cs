namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class PaymentPageViewModel
{
    public string? SearchTerm { get; set; }

    public List<CustomerSelectItem> Customers { get; set; } = new();

    public Guid? SelectedCustomerId { get; set; }
    public string? SelectedCustomerName { get; set; }
    public string? SelectedCustomerPhone { get; set; }

    public List<OrderListItemViewModel> CreditOrders { get; set; } = new();

    // Money-only ledger: one row per day a payment was made, newest first.
    public List<CustomerPaymentLogItem> PaymentsLog { get; set; } = new();

    public decimal TotalDue => CreditOrders.Sum(o => o.RemainingBalance);
    public decimal TotalOrdersAmount => CreditOrders.Sum(o => o.Total);
    public decimal TotalPaidSoFar => CreditOrders.Sum(o => o.AmountPaid);
}

public class CustomerPaymentLogItem
{
    public DateTime Date { get; set; }

    // Total amount paid across all of the customer's credit orders on this day
    public decimal AmountPaid { get; set; }

    // Remaining balance on the customer's total credit right after this day's payment(s)
    public decimal RemainingBalance { get; set; }
}

public class CustomerSelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal TotalCreditDue { get; set; }
}
