using Awlad_Zamzam.MVC.Models.ViewModels;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

/// <summary>
/// Admin-facing layer on top of <see cref="IDataRetentionService"/>. Holds the list of
/// "watched" tables (currently just Orders) and never deletes anything without an explicit,
/// admin-approved call to <see cref="DeleteAsync"/> — nothing here runs automatically.
/// </summary>
public interface IDataCleanupService
{
    /// <summary>Returns every watched table that has crossed its threshold and needs admin attention.</summary>
    Task<List<DataCleanupOverviewItem>> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the admin-approved <paramref name="requestedCount"/> oldest eligible rows from
    /// the watched table identified by <paramref name="key"/>. The actual eligible count is
    /// re-checked at the moment of deletion, so this never deletes more than what's genuinely
    /// safe to delete right now — even if <paramref name="requestedCount"/> is higher.
    /// Returns the number of rows actually deleted.
    /// </summary>
    Task<int> DeleteAsync(string key, int requestedCount, CancellationToken cancellationToken = default);
}
