using Awlad_Zamzam.MVC.Models.ViewModels;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

public interface IOfferService
{
    Task<List<OfferViewModel>> GetActiveOffersAsync();
    Task<List<OfferViewModel>> GetAllForAdminAsync();
    Task<OfferFormViewModel> GetForCreateAsync();
    Task<OfferFormViewModel?> GetForEditAsync(Guid id);
    Task<Guid> CreateAsync(OfferFormViewModel model);
    Task UpdateAsync(OfferFormViewModel model);
    Task DeleteAsync(Guid id);
    Task ToggleActiveAsync(Guid id);
}
