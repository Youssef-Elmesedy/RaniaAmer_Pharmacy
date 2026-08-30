using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface IOrderRepository : IReadRepository<Order>, IWriteRepository<Order>
{
    Task<IReadOnlyList<Order>> GetAllWithDetailsAsync();

    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedWithDetailsAsync(
        string? searchTerm, string sortOrder, int pageNumber, int pageSize);

    // Filtered at the database level (not "load everything, filter in memory") — used by the
    // admin notifications feed, which only ever needs pending orders.
    Task<IReadOnlyList<Order>> GetPendingWithDetailsAsync();

    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId);

    Task<Order?> GetByIdWithDetailsAsync(Guid id);

    Task<int> CountPendingAsync();

    // Inserts a payment directly (bypassing the Order aggregate's tracked navigation collection)
    Task AddPaymentAsync(OrderPayment payment);

    // Aggregate totals across all credit ("آجل") orders, for the dashboard
    Task<(decimal TotalOutstanding, decimal TotalPaid)> GetCreditTotalsAsync();
}
