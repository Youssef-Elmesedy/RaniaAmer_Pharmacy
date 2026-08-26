using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class DataRetentionService : IDataRetentionService
{
    // Rows pulled per round-trip while scanning oldest-first — keeps memory bounded even when
    // the table has hundreds of thousands of rows.
    private const int ScanBatchSize = 2000;

    // Rows deleted per SaveChanges call, so one delete pass isn't a single giant transaction.
    private const int DeleteBatchSize = 500;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(ApplicationDbContext context, ILogger<DataRetentionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<int> CountAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        => _context.Set<TEntity>().CountAsync(cancellationToken);

    public async Task<int> CountEligibleOldestAsync<TEntity>(
        int scanCap,
        Func<TEntity, bool>? isEligibleForDeletion = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeChildren = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var eligibleCount = 0;
        DateTime? lastSeenCreatedAt = null;

        while (eligibleCount < scanCap && !cancellationToken.IsCancellationRequested)
        {
            var batch = await FetchNextBatchAsync(lastSeenCreatedAt, includeChildren, cancellationToken);
            if (batch.Count == 0) break; // scanned the whole table

            lastSeenCreatedAt = batch[^1].CreatedAt;

            var eligibleInBatch = isEligibleForDeletion == null
                ? batch.Count
                : batch.Count(isEligibleForDeletion);

            eligibleCount = Math.Min(scanCap, eligibleCount + eligibleInBatch);
        }

        return eligibleCount;
    }

    public async Task<int> DeleteOldestAsync<TEntity>(
        int countToDelete,
        Func<TEntity, bool>? isEligibleForDeletion = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeChildren = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        if (countToDelete <= 0) return 0;

        var set = _context.Set<TEntity>();
        var deletedCount = 0;
        DateTime? lastSeenCreatedAt = null;

        // Keyset-scan forward through the table oldest-first. We scan strictly by CreatedAt
        // regardless of what gets deleted along the way, so this always terminates: either we
        // hit countToDelete, or we run out of rows to scan.
        while (deletedCount < countToDelete && !cancellationToken.IsCancellationRequested)
        {
            var batch = await FetchNextBatchAsync(lastSeenCreatedAt, includeChildren, cancellationToken);
            if (batch.Count == 0) break;

            lastSeenCreatedAt = batch[^1].CreatedAt;

            var eligible = isEligibleForDeletion == null
                ? batch
                : batch.Where(isEligibleForDeletion).ToList();

            foreach (var chunk in eligible.Chunk(DeleteBatchSize))
            {
                if (deletedCount >= countToDelete) break;

                var take = Math.Min(chunk.Length, countToDelete - deletedCount);
                var ids = chunk.Take(take).Select(e => e.Id).ToList();

                var tracked = await set.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
                set.RemoveRange(tracked);
                await _context.SaveChangesAsync(cancellationToken);

                deletedCount += tracked.Count;
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "Data retention: deleted {Count} old {Entity} rows (admin-approved).",
                deletedCount, typeof(TEntity).Name);
        }

        return deletedCount;
    }

    private async Task<List<TEntity>> FetchNextBatchAsync<TEntity>(
        DateTime? after,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeChildren,
        CancellationToken cancellationToken)
        where TEntity : BaseEntity
    {
        IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking().OrderBy(e => e.CreatedAt);

        if (after.HasValue)
        {
            var afterValue = after.Value;
            query = query.Where(e => e.CreatedAt > afterValue);
        }

        if (includeChildren != null)
            query = includeChildren(query);

        return await query.Take(ScanBatchSize).ToListAsync(cancellationToken);
    }
}
