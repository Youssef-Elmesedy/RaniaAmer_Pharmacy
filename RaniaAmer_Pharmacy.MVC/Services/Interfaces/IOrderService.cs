using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface IOrderService
{
    Task<Guid> CreateFromCartAsync(CheckoutViewModel model, Guid? authenticatedCustomerId = null);
    Task<Guid> CreateCreditOrderByAdminAsync(CreditOrderFormViewModel model);
    Task<List<OrderListItemViewModel>> GetAllAsync();
    Task<AdminOrderListViewModel> GetPagedAsync(string? searchTerm, string sortOrder, int pageNumber, int pageSize);
    Task<List<OrderListItemViewModel>> GetPendingAsync();
    Task<List<OrderListItemViewModel>> GetByCustomerAsync(Guid customerId);
    Task<List<OrderListItemViewModel>> GetCreditOrdersByCustomerAsync(Guid customerId);
    Task<List<CustomerPaymentLogItem>> GetPaymentsLogByCustomerAsync(Guid customerId);
    Task<OrderDetailsViewModel?> GetDetailsAsync(Guid id);
    Task CompleteAsync(Guid id, bool isCredit);
    Task CancelAsync(Guid id);
    Task AddPaymentAsync(Guid orderId, decimal amount, string? notes);
    Task PayCustomerCreditAsync(Guid customerId, decimal amount, string? notes);
    Task DeleteAsync(Guid id);
    Task<int> GetPendingCountAsync();
}
