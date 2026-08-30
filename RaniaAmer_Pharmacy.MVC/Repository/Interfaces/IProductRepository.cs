using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface IProductRepository : IReadRepository<Product>, IWriteRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllIncludeCategory();
    Task<Product?> GetByIdWithCategoryAsync(Guid id);
    Task<Product?> GetByIdWithDetailsAsync(Guid id);
    Task<Product?> GetByIdWithUnitOptionsAsync(Guid id);
    Task<IReadOnlyList<Product>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids);
    void MarkUnitOptionsAsAdded(IEnumerable<ProductUnitOption> options);

    Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId,
        string? searchTerm,
        string sortOrder,
        int pageNumber,
        int pageSize);

    Task<IReadOnlyList<Product>> GetOffersAsync(int take);
}
