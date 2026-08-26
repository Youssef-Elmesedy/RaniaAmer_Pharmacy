using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Enums;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class DataCleanupService : IDataCleanupService
{
    private readonly IDataRetentionService _retention;
    private readonly List<ICleanupPolicy> _policies;

    public DataCleanupService(IDataRetentionService retention)
    {
        _retention = retention;

        // ---- The list of tables being watched for cleanup. ----
        // To watch a NEW table in the future, add one more CleanupPolicy<T> entry here —
        // nothing else in the app needs to change.
        _policies = new List<ICleanupPolicy>
        {
            new CleanupPolicy<Order>
            {
                Key = "Orders",
                DisplayName = "الطلبات",
                Threshold = 150_000,
                MaxDeleteCap = 50_000,
                // Never delete a Pending order (still needs admin action) or an unpaid credit
                // ("آجل") order (would silently erase an outstanding debt).
                // Safe to delete: fully-settled completed orders, or cancelled ones (never
                // Pending - still needs admin action; never an unpaid credit order - would
                // erase a real debt).
                IsEligible = o =>
                    o.Status == OrderStatus.Cancelled ||
                    (o.Status == OrderStatus.Completed && (!o.IsCredit || o.IsFullyPaid)),
                IncludeChildren = q => q.Include(o => o.Items).Include(o => o.Payments)
            }
        };
    }

    public async Task<List<DataCleanupOverviewItem>> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<DataCleanupOverviewItem>();

        foreach (var policy in _policies)
        {
            var total = await policy.CountAsync(_retention, cancellationToken);
            if (total < policy.Threshold) continue;

            var eligible = await policy.CountEligibleAsync(_retention, cancellationToken);

            results.Add(new DataCleanupOverviewItem
            {
                Key = policy.Key,
                DisplayName = policy.DisplayName,
                TotalCount = total,
                Threshold = policy.Threshold,
                EligibleCount = eligible,
                MaxDeleteCap = policy.MaxDeleteCap
            });
        }

        return results;
    }

    public async Task<int> DeleteAsync(string key, int requestedCount, CancellationToken cancellationToken = default)
    {
        var policy = _policies.FirstOrDefault(p => p.Key == key)
            ?? throw new BusinessException("جدول غير معروف.", nameof(key));

        if (requestedCount <= 0)
            throw new BusinessException("العدد المطلوب حذفه يجب أن يكون أكبر من صفر.", nameof(requestedCount));

        // Re-check the real eligible count right now (defense against a stale page / race
        // condition) — never delete more than what's actually eligible, no matter what was requested.
        var eligibleNow = await policy.CountEligibleAsync(_retention, cancellationToken);
        var countToDelete = Math.Min(requestedCount, eligibleNow);

        if (countToDelete <= 0)
            throw new BusinessException("لا توجد عناصر مؤهلة للحذف حاليًا.", nameof(requestedCount));

        return await policy.DeleteAsync(_retention, countToDelete, cancellationToken);
    }

    // --- Internal plumbing: lets a strongly-typed CleanupPolicy<TEntity> be stored in one
    // non-generic list, since the entity type differs per watched table. ---

    private interface ICleanupPolicy
    {
        string Key { get; }
        string DisplayName { get; }
        int Threshold { get; }
        int MaxDeleteCap { get; }

        Task<int> CountAsync(IDataRetentionService retention, CancellationToken ct);
        Task<int> CountEligibleAsync(IDataRetentionService retention, CancellationToken ct);
        Task<int> DeleteAsync(IDataRetentionService retention, int count, CancellationToken ct);
    }

    private class CleanupPolicy<TEntity> : ICleanupPolicy where TEntity : BaseEntity
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        public required int Threshold { get; init; }
        public required int MaxDeleteCap { get; init; }
        public Func<TEntity, bool>? IsEligible { get; init; }
        public Func<IQueryable<TEntity>, IQueryable<TEntity>>? IncludeChildren { get; init; }

        public Task<int> CountAsync(IDataRetentionService retention, CancellationToken ct) =>
            retention.CountAsync<TEntity>(ct);

        public Task<int> CountEligibleAsync(IDataRetentionService retention, CancellationToken ct) =>
            retention.CountEligibleOldestAsync(MaxDeleteCap, IsEligible, IncludeChildren, ct);

        public Task<int> DeleteAsync(IDataRetentionService retention, int count, CancellationToken ct) =>
            retention.DeleteOldestAsync(count, IsEligible, IncludeChildren, ct);
    }
}
