namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class CustomerListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool HasAccount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastActivityAt { get; set; }
    public int OrdersCount { get; set; }
    public int CreditOrdersCount { get; set; }
    public decimal TotalCreditDue { get; set; }
}
