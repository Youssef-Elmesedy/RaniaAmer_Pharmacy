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

        var answerHash = _passwordHasher.HashPassword(customer, NormalizeAnswer(model.SecurityAnswer));
        customer.SetSecurityQuestion(model.SecurityQuestion, answerHash);

        await _customerRepository.SaveChangesAsync();

        return customer;
    }

    public async Task<Customer?> ValidateLoginAsync(CustomerLoginViewModel model)
    {
        var customer = await _customerRepository.GetByPhoneAsync(model.PhoneNumber.Trim());

        if (customer == null || !customer.HasAccount)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash!, model.Password);

        if (result != PasswordVerificationResult.Success && result != PasswordVerificationResult.SuccessRehashNeeded)
            return null;

        if (!customer.IsActive)
            throw new BusinessException(
                "تم إيقاف حسابك مؤقتًا لعدم النشاط لفترة طويلة، من فضلك تواصل معنا لإعادة تفعيله.",
                nameof(model.PhoneNumber));

        customer.RecordActivity();
        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();

        return customer;
    }

    public async Task<string?> GetSecurityQuestionAsync(string phoneNumber)
    {
        var customer = await _customerRepository.GetByPhoneAsync(phoneNumber.Trim());

        if (customer == null || !customer.HasAccount || !customer.HasSecurityQuestion)
            return null;

        return customer.SecurityQuestion;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        var customer = await _customerRepository.GetByPhoneAsync(model.PhoneNumber.Trim());

        if (customer == null || !customer.HasAccount || !customer.HasSecurityQuestion)
            return false;

        var answerResult = _passwordHasher.VerifyHashedPassword(
            customer, customer.SecurityAnswerHash!, NormalizeAnswer(model.SecurityAnswer));

        if (answerResult != PasswordVerificationResult.Success && answerResult != PasswordVerificationResult.SuccessRehashNeeded)
            return false;

        var newHash = _passwordHasher.HashPassword(customer, model.NewPassword);
        customer.SetPassword(newHash);

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();

        return true;
    }

    public Task<Customer?> GetProfileAsync(Guid customerId) => _customerRepository.GetByIdAsync(customerId);

    public async Task UpdateProfileAsync(Guid customerId, EditProfileViewModel model)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new BusinessException("العميل غير موجود", nameof(customerId));

        var phone = model.PhoneNumber.Trim();

        if (phone != customer.PhoneNumber)
        {
            var phoneTaken = await _customerRepository.ExistsByPhoneAsync(phone);
            if (phoneTaken)
                throw new BusinessException("رقم الهاتف مستخدم بالفعل من حساب آخر", nameof(model.PhoneNumber));
        }

        customer.Update(model.Name, phone, model.Address);

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid customerId, ChangePasswordViewModel model)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new BusinessException("العميل غير موجود", nameof(customerId));

        if (!customer.HasAccount)
            throw new BusinessException("لا يوجد حساب لهذا العميل", nameof(customerId));

        var currentResult = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash!, model.CurrentPassword);

        if (currentResult != PasswordVerificationResult.Success && currentResult != PasswordVerificationResult.SuccessRehashNeeded)
            throw new BusinessException("كلمة المرور الحالية غير صحيحة", nameof(model.CurrentPassword));

        var newHash = _passwordHasher.HashPassword(customer, model.NewPassword);
        customer.SetPassword(newHash);

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();
    }

    public async Task ChangeSecurityQuestionAsync(Guid customerId, ChangeSecurityQuestionViewModel model)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new BusinessException("العميل غير موجود", nameof(customerId));

        if (!customer.HasAccount)
            throw new BusinessException("لا يوجد حساب لهذا العميل", nameof(customerId));

        var currentResult = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash!, model.CurrentPassword);

        if (currentResult != PasswordVerificationResult.Success && currentResult != PasswordVerificationResult.SuccessRehashNeeded)
            throw new BusinessException("كلمة المرور الحالية غير صحيحة", nameof(model.CurrentPassword));

        var answerHash = _passwordHasher.HashPassword(customer, NormalizeAnswer(model.SecurityAnswer));
        customer.SetSecurityQuestion(model.SecurityQuestion, answerHash);

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();
    }

    private static string NormalizeAnswer(string answer) => answer.Trim().ToUpperInvariant();
}
