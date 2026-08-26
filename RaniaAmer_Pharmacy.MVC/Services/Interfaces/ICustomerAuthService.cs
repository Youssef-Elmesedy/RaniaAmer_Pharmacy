using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface ICustomerAuthService
{
    Task<Customer> RegisterAsync(CustomerRegisterViewModel model);
    Task<Customer?> ValidateLoginAsync(CustomerLoginViewModel model);

    // Forgot / reset password (via security question, no login required)
    Task<string?> GetSecurityQuestionAsync(string phoneNumber);
    Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);

    // Profile management (requires the customer to already be logged in)
    Task<Customer?> GetProfileAsync(Guid customerId);
    Task UpdateProfileAsync(Guid customerId, EditProfileViewModel model);
    Task ChangePasswordAsync(Guid customerId, ChangePasswordViewModel model);
    Task ChangeSecurityQuestionAsync(Guid customerId, ChangeSecurityQuestionViewModel model);
}
