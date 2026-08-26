using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class SaleUnitService : ISaleUnitService
{
    private const string SaleUnitsCacheKey = "sale-units:select-list";

    private readonly ISaleUnitRepository _saleUnitRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _cache;
    private readonly ICatalogChangeTracker _catalogChangeTracker;

    public SaleUnitService(
        ISaleUnitRepository saleUnitRepository,
        IProductRepository productRepository,
        IMemoryCache cache,
        ICatalogChangeTracker catalogChangeTracker)
    {
        _saleUnitRepository = saleUnitRepository;
        _productRepository = productRepository;
        _cache = cache;
        _catalogChangeTracker = catalogChangeTracker;
    }

    public async Task<List<SaleUnitViewModel>> GetAllAsync()
    {
        var units = await _saleUnitRepository.GetAllWithProductsAsync();

        return units.Select(u => new SaleUnitViewModel
        {
            Id = u.Id,
            Name = u.Name,
            ProductsCount = u.Products.Count
        }).ToList();
    }

    // Cached because this is read on every admin product form load
    public async Task<List<SaleUnitSelectItem>> GetSelectListAsync()
    {
        if (_cache.TryGetValue(SaleUnitsCacheKey, out List<SaleUnitSelectItem>? cached) && cached != null)
            return cached;

        var units = await _saleUnitRepository.GetAllAsync();

        var result = units
            .OrderBy(u => u.Name)
            .Select(u => new SaleUnitSelectItem { Id = u.Id, Name = u.Name })
            .ToList();

        _cache.Set(SaleUnitsCacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<SaleUnitFormViewModel?> GetForEditAsync(Guid id)
    {
        var unit = await _saleUnitRepository.GetByIdAsync(id);
        if (unit == null) return null;

        return new SaleUnitFormViewModel
        {
            Id = unit.Id,
            Name = unit.Name
        };
    }

    public async Task<Guid> CreateAsync(SaleUnitFormViewModel model)
    {
        if (await _saleUnitRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant()))
            throw new BusinessException("توجد وحدة بيع بهذا الاسم بالفعل", nameof(model.Name));

        var unit = SaleUnit.Create(model.Name);

        await _saleUnitRepository.AddAsync(unit);
        await _saleUnitRepository.SaveChangesAsync();

        InvalidateCache();

        return unit.Id;
    }

    public async Task UpdateAsync(SaleUnitFormViewModel model)
    {
        var unit = await _saleUnitRepository.GetByIdAsync(model.Id)
            ?? throw new BusinessException("وحدة البيع غير موجودة", nameof(model.Id));

        if (await _saleUnitRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant(), model.Id))
            throw new BusinessException("توجد وحدة بيع أخرى بهذا الاسم بالفعل", nameof(model.Name));

        unit.Update(model.Name);

        await _saleUnitRepository.UpdateAsync(unit);
        await _saleUnitRepository.SaveChangesAsync();

        InvalidateCache();
    }

    public async Task DeleteAsync(Guid id)
    {
        var unit = await _saleUnitRepository.GetByIdAsync(id)
            ?? throw new BusinessException("وحدة البيع غير موجودة", nameof(id));

        var isUsed = await _productRepository.AnyAsync(p => p.SaleUnitId == id)
            || await _saleUnitRepository.IsUsedAsSubUnitAsync(id);

        if (isUsed)
            throw new BusinessException("لا يمكن حذف وحدة البيع لوجود منتجات تستخدمها", nameof(id));

        await _saleUnitRepository.DeleteAsync(unit);
        await _saleUnitRepository.SaveChangesAsync();

        InvalidateCache();
    }

    private void InvalidateCache()
    {
        _cache.Remove(SaleUnitsCacheKey);
        _catalogChangeTracker.Touch();
    }
}
