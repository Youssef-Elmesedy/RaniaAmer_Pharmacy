using Awlad_Zamzam.MVC.Models.Entities;

namespace Awlad_Zamzam.MVC.Repository.Interfaces;

public interface ICategoryRepository : IReadRepository<Category>, IWriteRepository<Category>
{
    Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);

    Task<IReadOnlyList<Category>> GetAllWithProductsAsync();
}
