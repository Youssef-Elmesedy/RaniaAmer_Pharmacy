using Awlad_Zamzam.MVC.Models.Entities;
using System.Linq.Expressions;

namespace Awlad_Zamzam.MVC.Repository.Interfaces;

public interface IReadRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<TEntity>> GetAllAsync();

    Task<bool> ExistsAsync(Guid id);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

    IQueryable<TEntity> Query();
}