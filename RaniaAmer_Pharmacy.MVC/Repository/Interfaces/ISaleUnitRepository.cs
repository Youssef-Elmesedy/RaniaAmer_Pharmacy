using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface ISaleUnitRepository : IReadRepository<SaleUnit>, IWriteRepository<SaleUnit>
{
    Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null);

    Task<IReadOnlyList<SaleUnit>> GetAllWithProductsAsync();

    Task<bool> IsUsedAsSubUnitAsync(Guid saleUnitId);
}
