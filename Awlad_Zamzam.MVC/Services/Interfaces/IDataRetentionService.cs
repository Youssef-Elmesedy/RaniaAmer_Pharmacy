using Awlad_Zamzam.MVC.Models.Entities;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

/// <summary>
/// Generic "table pruning" engine — reusable for ANY table that grows unbounded over time
/// (orders today, an activity log or audit trail added later, etc.). Every entity already has
/// <c>CreatedAt</c> via <see cref="BaseEntity"/>, so a new table needs no new engine code —
/// just a policy (threshold + eligibility rule) wired up wherever it's watched (see
/// IDataCleanupService for the admin-approval workflow built on top of this).
///
/// This engine never deletes anything on its own — it only counts candidates and, when
/// explicitly asked, deletes an admin-approved number of them. All destructive action requires
/// an explicit call to DeleteOldestAsync with a count the admin approved.
/// </summary>
public interface IDataRetentionService
{
    /// <summary>Total row count for <typeparamref name="TEntity"/>.</summary>
    Task<int> CountAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    /// <summary>
    /// Scans the oldest rows of <typeparamref name="TEntity"/> (up to <paramref name="scanCap"/>
    /// of them) and returns how many are eligible for deletion, without deleting anything.
    /// </summary>
    Task<int> CountEligibleOldestAsync<TEntity>(
        int scanCap,
        Func<TEntity, bool>? isEligibleForDeletion = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeChildren = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    /// <summary>
    /// Deletes up to <paramref name="countToDelete"/> of the oldest eligible rows of
    /// <typeparamref name="TEntity"/>. Meant to be called only after an admin has approved a
    /// specific count (typically the number returned by <see cref="CountEligibleOldestAsync{TEntity}"/>,
    /// or less). Returns the number of rows actually deleted.
    /// </summary>
    Task<int> DeleteOldestAsync<TEntity>(
        int countToDelete,
        Func<TEntity, bool>? isEligibleForDeletion = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeChildren = null,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;
}
