using Awlad_Zamzam.MVC.Models.ViewModels;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

public interface ICustomerService
{
    Task CreateAsync(CustomerViewModel model);

    Task<List<CustomerListItemViewModel>> SearchAsync(string? searchTerm);

    Task<CustomerListItemViewModel?> GetByIdAsync(Guid id);
}
