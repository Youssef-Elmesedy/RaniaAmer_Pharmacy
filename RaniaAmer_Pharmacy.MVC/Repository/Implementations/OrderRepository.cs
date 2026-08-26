using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Enums;
using RaniaAmer_Pharmacy.MVC.Repository.Generic;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Repository.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<Order> _read;
    private readonly WriteRepository<Order> _write;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<Order>(context);
        _write = new WriteRepository<Order>(context);
    }

    public Task<Order?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<Order>> GetAllAsync() => _read.GetAllAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Order, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Order, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<Order> Query() => _read.Query();

    public Task AddAsync(Order entity) => _write.AddAsync(entity);

    public Task UpdateAsync(Order entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(Order entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public async Task<IReadOnlyList<Order>> GetAllWithDetailsAsync() =>
        await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId) =>
        await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

    public Task<int> CountPendingAsync() =>
        _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);

    public async Task AddPaymentAsync(OrderPayment payment) =>
        await _context.OrderPayments.AddAsync(payment);

    public async Task<(decimal TotalOutstanding, decimal TotalPaid)> GetCreditTotalsAsync()
    {
        var creditOrders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsNoTracking()
            .Where(o => o.IsCredit && o.Status == OrderStatus.Completed)
            .ToListAsync();

        var totalPaid = creditOrders.Sum(o => o.AmountPaid);
        var totalOutstanding = creditOrders.Sum(o => o.RemainingBalance);

        return (totalOutstanding, totalPaid);
    }
}
