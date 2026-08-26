using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface ICategoryRepository : IReadRepository<Category>, IWriteRepository<Category>
{
    Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);

    Task<IReadOnlyList<Category>> GetAllWithProductsAsync();
}
