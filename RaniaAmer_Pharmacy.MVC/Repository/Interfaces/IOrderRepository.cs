using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface IOrderRepository : IReadRepository<Order>, IWriteRepository<Order>
{
    Task<IReadOnlyList<Order>> GetAllWithDetailsAsync();

    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId);

    Task<Order?> GetByIdWithDetailsAsync(Guid id);

    Task<int> CountPendingAsync();

    // Inserts a payment directly (bypassing the Order aggregate's tracked navigation collection)
    Task AddPaymentAsync(OrderPayment payment);

    // Aggregate totals across all credit ("آجل") orders, for the dashboard
    Task<(decimal TotalOutstanding, decimal TotalPaid)> GetCreditTotalsAsync();
}
