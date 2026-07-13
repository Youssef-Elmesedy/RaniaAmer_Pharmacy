using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class CustomerService : ICustomerService
{
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
            OrdersCount = orders.Count,
            CreditOrdersCount = creditOrders.Count,
            TotalCreditDue = creditOrders.Sum(o => o.RemainingBalance)
        };
    }
}
