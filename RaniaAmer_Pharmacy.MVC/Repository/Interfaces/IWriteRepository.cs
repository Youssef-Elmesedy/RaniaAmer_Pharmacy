using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface IWriteRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary> Add Update Delete SaveChanges </summary>
    Task AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task<int> SaveChangesAsync();
}
