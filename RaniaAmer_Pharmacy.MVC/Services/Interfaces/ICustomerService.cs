using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface ICustomerService
{
    Task CreateAsync(CustomerViewModel model);

    Task<List<CustomerListItemViewModel>> SearchAsync(string? searchTerm);

    Task<CustomerListItemViewModel?> GetByIdAsync(Guid id);

    Task DeleteAsync(Guid id);

    // Inactivity auto-pause (customer inactive 3+ months) - admin-approval workflow, see DataCleanup
    int InactivityThresholdMonths { get; }
    Task<int> CountInactiveEligibleAsync();
    Task<List<CustomerListItemViewModel>> GetInactiveEligibleAsync();
    Task<int> DeactivateInactiveAsync();
    Task ReactivateAsync(Guid id);
}
