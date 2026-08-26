using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryViewModel>> GetAllAsync();
    Task<List<CategorySelectItem>> GetSelectListAsync();
    Task<CategoryFormViewModel?> GetForEditAsync(Guid id);
    Task<Guid> CreateAsync(CategoryFormViewModel model);
    Task UpdateAsync(CategoryFormViewModel model);
    Task DeleteAsync(Guid id);
}
