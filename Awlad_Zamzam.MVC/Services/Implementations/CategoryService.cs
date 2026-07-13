using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class CategoryService : ICategoryService
{
    private const string CategoriesCacheKey = "categories:select-list";

    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IImageService _imageService;
    private readonly IMemoryCache _cache;
    private readonly ICatalogChangeTracker _catalogChangeTracker;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IImageService imageService,
        IMemoryCache cache,
        ICatalogChangeTracker catalogChangeTracker)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _imageService = imageService;
        _cache = cache;
        _catalogChangeTracker = catalogChangeTracker;
    }

    public async Task<List<CategoryViewModel>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllWithProductsAsync();

        return categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Image = c.Image,
            ProductsCount = c.Products.Count
        }).ToList();
    }

    // Cached because this hits the DB on every single page load (navbar / footer / product filters)
    public async Task<List<CategorySelectItem>> GetSelectListAsync()
    {
        if (_cache.TryGetValue(CategoriesCacheKey, out List<CategorySelectItem>? cached) && cached != null)
            return cached;

        var categories = await _categoryRepository.GetAllAsync();

        var result = categories
            .OrderBy(c => c.Name)
            .Select(c => new CategorySelectItem { Id = c.Id, Name = c.Name })
            .ToList();

        _cache.Set(CategoriesCacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<CategoryFormViewModel?> GetForEditAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return null;

        return new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            ExistingImage = category.Image
        };
    }

    public async Task<Guid> CreateAsync(CategoryFormViewModel model)
    {
        if (await _categoryRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant()))
            throw new BusinessException("يوجد قسم بهذا الاسم بالفعل", nameof(model.Name));

        if (model.ImageFile == null)
            throw new BusinessException("صورة القسم مطلوبة", nameof(model.ImageFile));

        var imagePath = await _imageService.SaveImageAsync(model.ImageFile, "Categories");

        var category = Category.Create(model.Name, imagePath);

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        InvalidateCache();

        return category.Id;
    }

    public async Task UpdateAsync(CategoryFormViewModel model)
    {
        var category = await _categoryRepository.GetByIdAsync(model.Id)
            ?? throw new BusinessException("القسم غير موجود", nameof(model.Id));

        if (await _categoryRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant(), model.Id))
            throw new BusinessException("يوجد قسم آخر بهذا الاسم بالفعل", nameof(model.Name));

        var imagePath = category.Image;

        if (model.ImageFile != null)
        {
            _imageService.DeleteImage(category.Image);
            imagePath = await _imageService.SaveImageAsync(model.ImageFile, "Categories");
        }

        category.Update(model.Name, imagePath);

        await _categoryRepository.UpdateAsync(category);
        await _categoryRepository.SaveChangesAsync();

        InvalidateCache();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new BusinessException("القسم غير موجود", nameof(id));

        var hasProducts = await _productRepository.AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
            throw new BusinessException("لا يمكن حذف القسم لوجود منتجات مرتبطة به", nameof(id));

        _imageService.DeleteImage(category.Image);

        await _categoryRepository.DeleteAsync(category);
        await _categoryRepository.SaveChangesAsync();

        InvalidateCache();
    }

    private void InvalidateCache()
    {
        _cache.Remove(CategoriesCacheKey);
        _catalogChangeTracker.Touch();
    }
}
