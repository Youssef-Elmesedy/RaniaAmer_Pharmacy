using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface IBranchService
{
    // Public-facing: every branch, in display order (used by the footer + contact page)
    Task<List<BranchViewModel>> GetAllAsync();
    Task<BranchFormViewModel?> GetForEditAsync(Guid id);
    Task<Guid> CreateAsync(BranchFormViewModel model);
    Task UpdateAsync(BranchFormViewModel model);
    Task DeleteAsync(Guid id);
}
