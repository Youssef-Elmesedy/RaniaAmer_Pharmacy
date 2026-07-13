using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class CustomerAuthService : ICustomerAuthService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public CustomerAuthService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> RegisterAsync(CustomerRegisterViewModel model)
    {
        var existing = await _customerRepository.GetByPhoneAsync(model.PhoneNumber.Trim());

        Customer customer;

        if (existing == null)
        {
            customer = Customer.Create(model.Name, model.PhoneNumber, model.Address);
            await _customerRepository.AddAsync(customer);
        }
        else
        {
            if (existing.HasAccount)
                throw new BusinessException("رقم الهاتف مسجل بالفعل، يمكنك تسجيل الدخول مباشرة", nameof(model.PhoneNumber));

            existing.Update(model.Name, model.PhoneNumber, model.Address);
            customer = existing;
            await _customerRepository.UpdateAsync(customer);
        }

        var hash = _passwordHasher.HashPassword(customer, model.Password);
        customer.SetPassword(hash);

        await _customerRepository.SaveChangesAsync();

        return customer;
    }

    public async Task<Customer?> ValidateLoginAsync(CustomerLoginViewModel model)
    {
        var customer = await _customerRepository.GetByPhoneAsync(model.PhoneNumber.Trim());

        if (customer == null || !customer.HasAccount)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash!, model.Password);

        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded
            ? customer
            : null;
    }
}
