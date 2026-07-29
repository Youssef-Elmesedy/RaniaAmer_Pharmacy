using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Awlad_Zamzam.MVC.Repository.Generic;

public class WriteRepository<TEntity> : IWriteRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly ILogger<WriteRepository<TEntity>> _logger;

    // logger is optional: this type is manually "new"'d (not DI-resolved) by each concrete
    // repository (CategoryRepository, OrderRepository, etc.), so it can't rely on constructor
    // injection for it. Falls back to a no-op logger when not supplied.
    public WriteRepository(ApplicationDbContext context, ILogger<WriteRepository<TEntity>>? logger = null)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = logger ?? NullLogger<WriteRepository<TEntity>>.Instance;
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
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var keyValue = entry.Metadata.FindPrimaryKey()?.Properties
                    .Select(p => entry.Property(p.Name).CurrentValue)
                    .FirstOrDefault();

                object? dbValues;
                try
                {
                    dbValues = (await entry.GetDatabaseValuesAsync())?.ToObject();
                }
                catch
                {
                    dbValues = "<failed to read>";
                }

                _logger.LogWarning(
                    "Concurrency conflict: entity={Entity} id={Id} EF-state={State} row-in-db={RowInDb}",
                    entry.Entity.GetType().Name,
                    keyValue,
                    entry.State,
                    dbValues == null ? "NO (row does not exist)" : "YES (row exists with different/matching values)");
            }

            throw;
        }
    }
}