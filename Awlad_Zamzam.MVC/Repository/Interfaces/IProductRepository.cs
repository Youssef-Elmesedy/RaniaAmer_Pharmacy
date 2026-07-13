using Awlad_Zamzam.MVC.Models.Entities;

namespace Awlad_Zamzam.MVC.Repository.Interfaces;

public interface IProductRepository : IReadRepository<Product>, IWriteRepository<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(Guid id);

    Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId,
        string? searchTerm,
        string sortOrder,
        int pageNumber,
        int pageSize);

    Task<IReadOnlyList<Product>> GetOffersAsync(int take);
}
