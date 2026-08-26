using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

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
