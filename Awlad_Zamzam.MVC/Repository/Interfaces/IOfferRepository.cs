using Awlad_Zamzam.MVC.Models.Entities;

namespace Awlad_Zamzam.MVC.Repository.Interfaces;

public interface IOfferRepository : IReadRepository<Offer>, IWriteRepository<Offer>
{
    Task<IReadOnlyList<Offer>> GetActiveWithItemsAsync();

    Task<Offer?> GetByIdWithItemsAsync(Guid id);

    /// <summary>
    /// Explicitly marks each of these OfferItems as newly-added in the change tracker.
    /// Needed because EF Core can't reliably tell a brand-new OfferItem (client-generated,
    /// already non-empty Guid key) apart from an existing row once it's only discovered via
    /// collection fixup on an already-tracked Offer — without this, EF may issue an UPDATE
    /// for a row that was never inserted, causing a 0-rows-affected concurrency exception.
    /// </summary>
    void MarkItemsAsAdded(IEnumerable<OfferItem> items);
}
