using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC.Repository.Generic;

public class WriteRepository<TEntity> : IWriteRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public WriteRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public Task UpdateAsync(TEntity entity)
    {
        // If the entity is already tracked (loaded earlier in this same request/DbContext),
        // calling DbSet.Update() again would force EF to re-walk the whole graph and can
        // wrongly mark freshly-created child entities (e.g. new OfferItems with a pre-set Guid key)
        // as "Modified" instead of "Added", causing a 0-rows-affected concurrency exception.
        // Only attach + mark modified when the entity isn't tracked yet (disconnected scenario).
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Update(entity);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}