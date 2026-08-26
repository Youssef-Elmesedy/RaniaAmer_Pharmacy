using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface ISaleUnitService
{
    Task<List<SaleUnitViewModel>> GetAllAsync();
    Task<List<SaleUnitSelectItem>> GetSelectListAsync();
    Task<SaleUnitFormViewModel?> GetForEditAsync(Guid id);
    Task<Guid> CreateAsync(SaleUnitFormViewModel model);
    Task UpdateAsync(SaleUnitFormViewModel model);
    Task DeleteAsync(Guid id);
}
