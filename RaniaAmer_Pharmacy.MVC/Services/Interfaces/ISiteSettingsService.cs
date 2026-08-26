using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface ISiteSettingsService
{
    // Always returns a usable settings object — creates a default row on first use if none exists.
    Task<StoreSettingsViewModel> GetAsync();
    Task UpdateAsync(StoreSettingsViewModel model);
}
