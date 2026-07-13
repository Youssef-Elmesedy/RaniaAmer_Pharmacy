using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.ViewModels;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

public interface ICustomerAuthService
{
    Task<Customer> RegisterAsync(CustomerRegisterViewModel model);
    Task<Customer?> ValidateLoginAsync(CustomerLoginViewModel model);
}
