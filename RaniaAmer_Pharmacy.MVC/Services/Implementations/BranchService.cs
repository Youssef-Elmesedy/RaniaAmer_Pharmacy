using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

// Cached because the branch list is read on every public page (footer) and the contact page.
public class BranchService : IBranchService
{
    private const string CacheKey = "branches:all";

    private readonly IBranchRepository _repository;
    private readonly IMemoryCache _cache;

    public BranchService(IBranchRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<BranchViewModel>> GetAllAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<BranchViewModel>? cached) && cached != null)
            return cached;

        var branches = await _repository.GetAllOrderedAsync();
        var result = branches.Select(MapToViewModel).ToList();

        _cache.Set(CacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<BranchFormViewModel?> GetForEditAsync(Guid id)
    {
        var branch = await _repository.GetByIdAsync(id);
        if (branch == null) return null;

        return new BranchFormViewModel
        {
            Id = branch.Id,
            Name = branch.Name,
            PhoneNumber = branch.PhoneNumber,
            Address = branch.Address,
            WorkingHours = branch.WorkingHours,
            DeliveryAreaText = branch.DeliveryAreaText,
            MapEmbedUrl = branch.MapEmbedUrl,
            MapDirectionsUrl = branch.MapDirectionsUrl,
            DisplayOrder = branch.DisplayOrder
        };
    }

    public async Task<Guid> CreateAsync(BranchFormViewModel model)
    {
        var branch = Branch.Create(
            model.Name, model.PhoneNumber, model.Address, model.WorkingHours,
            model.DeliveryAreaText, model.MapEmbedUrl, model.MapDirectionsUrl, model.DisplayOrder);

        await _repository.AddAsync(branch);
        await _repository.SaveChangesAsync();

        InvalidateCache();

        return branch.Id;
    }

    public async Task UpdateAsync(BranchFormViewModel model)
    {
        var branch = await _repository.GetByIdAsync(model.Id)
            ?? throw new BusinessException("الفرع غير موجود", nameof(model.Id));

        branch.Update(
            model.Name, model.PhoneNumber, model.Address, model.WorkingHours,
            model.DeliveryAreaText, model.MapEmbedUrl, model.MapDirectionsUrl, model.DisplayOrder);

        await _repository.UpdateAsync(branch);
        await _repository.SaveChangesAsync();

        InvalidateCache();
    }

    public async Task DeleteAsync(Guid id)
    {
        var branch = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException("الفرع غير موجود", nameof(id));

        var totalBranches = await _repository.CountAsync();
        if (totalBranches <= 1)
            throw new BusinessException("لا يمكن حذف آخر فرع متبقي — لازم يفضل فرع واحد على الأقل", nameof(id));

        await _repository.DeleteAsync(branch);
        await _repository.SaveChangesAsync();

        InvalidateCache();
    }

    private void InvalidateCache() => _cache.Remove(CacheKey);

    private static BranchViewModel MapToViewModel(Branch b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        PhoneNumber = b.PhoneNumber,
        Address = b.Address,
        WorkingHours = b.WorkingHours,
        DeliveryAreaText = b.DeliveryAreaText,
        MapEmbedUrl = b.MapEmbedUrl,
        MapDirectionsUrl = b.MapDirectionsUrl,
        DisplayOrder = b.DisplayOrder
    };
}
