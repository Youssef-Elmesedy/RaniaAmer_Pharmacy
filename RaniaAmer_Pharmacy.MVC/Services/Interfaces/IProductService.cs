using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface IProductService
{
    Task<ProductListViewModel> GetListAsync(Guid? categoryId, string? searchTerm, string sortOrder, int pageNumber, int pageSize);
    Task<IReadOnlyList<ProductViewModel>> GetOffersAsync(int take);
    Task<Product?> GetDetailsAsync(Guid id);
    Task<IReadOnlyList<Product>> GetAllForAdminAsync();
    Task<Guid> CreateAsync(ProductFormViewModel model);
    Task UpdateAsync(ProductFormViewModel model);
    Task DeleteAsync(Guid id);
}
