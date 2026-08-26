using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface IBranchRepository : IReadRepository<Branch>, IWriteRepository<Branch>
{
    Task<IReadOnlyList<Branch>> GetAllOrderedAsync();
}
