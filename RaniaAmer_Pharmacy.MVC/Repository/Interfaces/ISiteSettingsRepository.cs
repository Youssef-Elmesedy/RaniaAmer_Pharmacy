using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync();
    Task AddAsync(SiteSettings settings);
    Task UpdateAsync(SiteSettings settings);
    Task<int> SaveChangesAsync();
}
