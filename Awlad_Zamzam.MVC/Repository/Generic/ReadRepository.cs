using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Awlad_Zamzam.MVC.Repository.Generic;

public class ReadRepository<TEntity> : IReadRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet;

    public ReadRepository(ApplicationDbContext context)
    {
        _dbSet = context.Set<TEntity>();
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet
            .AnyAsync(x => x.Id == id);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();

        return await _dbSet.CountAsync(predicate);
    }

    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsNoTracking();
    }
}