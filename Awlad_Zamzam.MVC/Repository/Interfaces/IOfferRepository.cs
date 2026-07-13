using Awlad_Zamzam.MVC.Models.Entities;

namespace Awlad_Zamzam.MVC.Repository.Interfaces;

public interface IOfferRepository : IReadRepository<Offer>, IWriteRepository<Offer>
{
    Task<IReadOnlyList<Offer>> GetActiveWithItemsAsync();

    Task<Offer?> GetByIdWithItemsAsync(Guid id);
}
