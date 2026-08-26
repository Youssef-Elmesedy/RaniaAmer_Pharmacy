using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class CustomerService : ICustomerService
{
    // A customer with no orders and no login for this many months gets auto-paused.
    private const int InactivityThresholdMonths = 3;

    int ICustomerService.InactivityThresholdMonths => InactivityThresholdMonths;

    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;

    public CustomerService(ICustomerRepository customerRepository, IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }

    public async Task CreateAsync(CustomerViewModel model)
    {
        var customer = Customer.Create(model.Name, model.PhoneNumber, model.Address);

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();
    }

    public async Task<List<CustomerListItemViewModel>> SearchAsync(string? searchTerm)
    {
        var query = _customerRepository.Query();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToUpperInvariant();
            var digits = searchTerm.Trim();

            query = query.Where(c =>
                c.NormalizedName.Contains(normalized) ||
                c.PhoneNumber.Contains(digits));
        }

        var customers = await query.OrderBy(c => c.Name).ToListAsync();

        var result = new List<CustomerListItemViewModel>();
        foreach (var customer in customers)
            result.Add(await BuildListItemAsync(customer));

        return result;
    }

    public async Task<CustomerListItemViewModel?> GetByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer == null ? null : await BuildListItemAsync(customer);
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id)
            ?? throw new BusinessException("العميل غير موجود", nameof(id));

        var orders = await _orderRepository.GetByCustomerIdAsync(id);
        if (orders.Any())
            throw new BusinessException(
                "لا يمكن حذف هذا العميل لوجود طلبات مسجلة باسمه. يمكنك حذف العميل فقط إذا لم يكن لديه أي طلبات سابقة.",
                nameof(id));

        await _customerRepository.DeleteAsync(customer);
        await _customerRepository.SaveChangesAsync();
    }

    public async Task<int> CountInactiveEligibleAsync()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-InactivityThresholdMonths);

        return await _customerRepository.Query()
            .Where(c => c.IsActive && (c.LastActivityAt ?? c.CreatedAt) < cutoff)
            .CountAsync();
    }

    public async Task<List<CustomerListItemViewModel>> GetInactiveEligibleAsync()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-InactivityThresholdMonths);

        var customers = await _customerRepository.Query()
            .Where(c => c.IsActive && (c.LastActivityAt ?? c.CreatedAt) < cutoff)
            .OrderBy(c => c.LastActivityAt ?? c.CreatedAt)
            .ToListAsync();

        var result = new List<CustomerListItemViewModel>();
        foreach (var customer in customers)
            result.Add(await BuildListItemAsync(customer));

        return result;
    }

    // Admin-approved bulk pause of every customer currently eligible (see CountInactiveEligibleAsync).
    // Re-checks eligibility right before pausing each one, in case something changed since the
    // admin loaded the page. Returns how many were actually paused.
    public async Task<int> DeactivateInactiveAsync()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-InactivityThresholdMonths);

        var customers = await _customerRepository.Query()
            .Where(c => c.IsActive && (c.LastActivityAt ?? c.CreatedAt) < cutoff)
            .ToListAsync();

        foreach (var customer in customers)
            customer.Deactivate();

        await _customerRepository.SaveChangesAsync();
        return customers.Count;
    }

    public async Task ReactivateAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id)
            ?? throw new BusinessException("العميل غير موجود", nameof(id));

        customer.Activate();
        customer.RecordActivity(); // give them a fresh 3-month window starting now

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();
    }

    private async Task<CustomerListItemViewModel> BuildListItemAsync(Customer customer)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id);
        var creditOrders = orders.Where(o => o.IsCredit).ToList();

        return new CustomerListItemViewModel
        {
            Id = customer.Id,
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            HasAccount = customer.HasAccount,
            IsActive = customer.IsActive,
            LastActivityAt = customer.LastActivityAt,
            OrdersCount = orders.Count,
            CreditOrdersCount = creditOrders.Count,
            TotalCreditDue = creditOrders.Sum(o => o.RemainingBalance)
        };
    }
}
