using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IImageService _imageService;
    private readonly ICatalogChangeTracker _catalogChangeTracker;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IOfferRepository offerRepository,
        IImageService imageService,
        ICatalogChangeTracker catalogChangeTracker)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _offerRepository = offerRepository;
        _imageService = imageService;
        _catalogChangeTracker = catalogChangeTracker;
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
        var offers = await _productRepository.GetOffersAsync(take);
        return offers.Select(MapToViewModel).ToList();
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
            model.Name, model.Description, model.Price, model.SaleUnit, model.DiscountPercentage, imagePath, category.Id);

        if (!model.IsAvailable)
            product.MarkAsUnavailable();

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _catalogChangeTracker.Touch();

        return product.Id;
    }

    public async Task UpdateAsync(ProductFormViewModel model)
    {
        var product = await _productRepository.GetByIdAsync(model.Id)
            ?? throw new BusinessException("المنتج غير موجود", nameof(model.Id));

        if (await _productRepository.ExistsByNameAsync(model.Name.Trim().ToUpperInvariant(), model.Id))
            throw new BusinessException("يوجد منتج آخر بهذا الاسم بالفعل", nameof(model.Name));

        var imagePath = product.ImagePath;

        if (model.ImageFile != null)
        {
            _imageService.DeleteImage(product.ImagePath);
            imagePath = await _imageService.SaveImageAsync(model.ImageFile, "Products");
        }

        product.Update(model.Name, model.Description, model.Price, model.SaleUnit, model.DiscountPercentage, imagePath, model.CategoryId);

        if (model.IsAvailable)
            product.MarkAsAvailable();
        else
            product.MarkAsUnavailable();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _catalogChangeTracker.Touch();
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
    }

    private static ProductViewModel MapToViewModel(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        SaleUnit = p.SaleUnit,
        DiscountPercentage = p.DiscountPercentage,
        ImagePath = p.ImagePath,
        IsAvailable = p.IsAvailable,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty
    };
}
