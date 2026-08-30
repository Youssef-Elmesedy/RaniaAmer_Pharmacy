using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class ProductService : IProductService
{
    private const string OffersCacheKey = "products:offers";

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IImageService _imageService;
    private readonly ICatalogChangeTracker _catalogChangeTracker;
    private readonly IMemoryCache _cache;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IOfferRepository offerRepository,
        IImageService imageService,
        ICatalogChangeTracker catalogChangeTracker,
        IMemoryCache cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _offerRepository = offerRepository;
        _imageService = imageService;
        _catalogChangeTracker = catalogChangeTracker;
        _cache = cache;
    }

    public async Task<ProductListViewModel> GetListAsync(
        Guid? categoryId, string? searchTerm, string sortOrder, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _productRepository.GetFilteredAsync(
            categoryId, searchTerm, sortOrder, pageNumber, pageSize);

        var categories = await _categoryRepository.GetAllAsync();

        return new ProductListViewModel
        {
            Products = items.Select(MapToViewModel).ToList(),
            Categories = categories.Select(c => new CategorySelectItem { Id = c.Id, Name = c.Name }).ToList(),
            CurrentCategoryId = categoryId,
            SearchTerm = searchTerm,
            SortOrder = sortOrder,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<ProductViewModel>> GetOffersAsync(int take)
    {
        // Cache one reasonably-large batch (not keyed by "take") so every caller shares the
        // same cached query regardless of how many they ask for, and slice in-memory.
        const int cachedBatchSize = 24;

        if (!_cache.TryGetValue(OffersCacheKey, out List<ProductViewModel>? cached) || cached == null)
        {
            var offers = await _productRepository.GetOffersAsync(cachedBatchSize);
            cached = offers.Select(MapToViewModel).ToList();

            _cache.Set(OffersCacheKey, cached, TimeSpan.FromMinutes(10));
        }

        return cached.Take(take).ToList();
    }

    public Task<Product?> GetDetailsAsync(Guid id) => _productRepository.GetByIdWithCategoryAsync(id);

    public Task<IReadOnlyList<Product>> GetAllForAdminAsync() => _productRepository.GetAllIncludeCategory();

    public async Task<Guid> CreateAsync(ProductFormViewModel model)
    {
        if (await _productRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant()))
            throw new BusinessException("يوجد منتج بهذا الاسم بالفعل", nameof(model.Name));

        var category = await _categoryRepository.GetByIdAsync(model.CategoryId)
            ?? throw new BusinessException("القسم المحدد غير موجود", nameof(model.CategoryId));

        var imagePath = model.ImageFile != null
            ? await _imageService.SaveImageAsync(model.ImageFile, "Products")
            : null;

        var product = Product.Create(
            model.Name, model.Description, model.Price, model.SaleUnitId, model.DiscountPercentage, imagePath, category.Id);

        product.ReplaceUnitOptions(BuildUnitOptionInputs(model));

        if (!model.IsAvailable)
            product.MarkAsUnavailable();

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _catalogChangeTracker.Touch();
        _cache.Remove(OffersCacheKey);

        return product.Id;
    }

    public async Task UpdateAsync(ProductFormViewModel model)
    {
        // Tracked (not AsNoTracking) so ReplaceUnitOptions()'s changes are picked up directly
        // by SaveChanges, without needing to re-attach/Update() the whole graph.
        var product = await _productRepository.GetByIdWithUnitOptionsAsync(model.Id)
            ?? throw new BusinessException("المنتج غير موجود", nameof(model.Id));

        if (await _productRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant(), model.Id))
            throw new BusinessException("يوجد منتج آخر بهذا الاسم بالفعل", nameof(model.Name));

        var imagePath = product.ImagePath;

        if (model.ImageFile != null)
        {
            _imageService.DeleteImage(product.ImagePath);
            imagePath = await _imageService.SaveImageAsync(model.ImageFile, "Products");
        }

        product.Update(model.Name, model.Description, model.Price, model.SaleUnitId, model.DiscountPercentage, imagePath, model.CategoryId);

        var newOptions = product.ReplaceUnitOptions(BuildUnitOptionInputs(model));
        _productRepository.MarkUnitOptionsAsAdded(newOptions);

        if (model.IsAvailable)
            product.MarkAsAvailable();
        else
            product.MarkAsUnavailable();

        await _productRepository.SaveChangesAsync();

        _catalogChangeTracker.Touch();
        _cache.Remove(OffersCacheKey);
    }

    private static IEnumerable<(Guid SaleUnitId, int QuantityPerBaseUnit)> BuildUnitOptionInputs(ProductFormViewModel model)
    {
        if (model.UnitOptionSaleUnitIds == null) yield break;

        for (var i = 0; i < model.UnitOptionSaleUnitIds.Count; i++)
        {
            var saleUnitId = model.UnitOptionSaleUnitIds[i];
            var quantity = model.UnitOptionQuantities?.ElementAtOrDefault(i) ?? 0;

            if (saleUnitId != Guid.Empty && quantity > 1)
                yield return (saleUnitId, quantity);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new BusinessException("المنتج غير موجود", nameof(id));

        var isInOffer = await _offerRepository.Query()
            .SelectMany(o => o.Items)
            .AnyAsync(i => i.ProductId == id);

        if (isInOffer)
            throw new BusinessException("لا يمكن حذف المنتج لأنه جزء من عرض قائم، احذفه من العرض أولاً", nameof(id));

        _imageService.DeleteImage(product.ImagePath);

        await _productRepository.DeleteAsync(product);
        await _productRepository.SaveChangesAsync();

        _catalogChangeTracker.Touch();
        _cache.Remove(OffersCacheKey);
    }

    private static ProductViewModel MapToViewModel(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        SaleUnitId = p.SaleUnitId,
        SaleUnitName = p.SaleUnit?.Name ?? string.Empty,
        DiscountPercentage = p.DiscountPercentage,
        ImagePath = p.ImagePath,
        IsAvailable = p.IsAvailable,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty
    };
}
