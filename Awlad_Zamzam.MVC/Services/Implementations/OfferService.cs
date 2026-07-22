using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class OfferService : IOfferService
{
    private const string ActiveOffersCacheKey = "offers:active-list";

    private readonly IOfferRepository _offerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _cache;
    private readonly ICatalogChangeTracker _catalogChangeTracker;
    public readonly ApplicationDbContext _context;
    public OfferService(
        IOfferRepository offerRepository,
        IProductRepository productRepository,
        IMemoryCache cache,
        ICatalogChangeTracker catalogChangeTracker,
        ApplicationDbContext context)
    {
        _offerRepository = offerRepository;
        _productRepository = productRepository;
        _cache = cache;
        _catalogChangeTracker = catalogChangeTracker;
        _context = context;
    }

    public async Task<List<OfferViewModel>> GetActiveOffersAsync()
    {
        if (_cache.TryGetValue(ActiveOffersCacheKey, out List<OfferViewModel>? cached) && cached != null)
            return cached;

        var offers = await _offerRepository.GetActiveWithItemsAsync();
        var result = offers.Select(MapToViewModel).ToList();

        _cache.Set(ActiveOffersCacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<List<OfferViewModel>> GetAllForAdminAsync()
    {
        var offers = await _offerRepository.Query()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = new List<OfferViewModel>();
        foreach (var offer in offers)
        {
            var full = await _offerRepository.GetByIdWithItemsAsync(offer.Id);
            if (full != null) result.Add(MapToViewModel(full));
        }

        return result;
    }

    public async Task<OfferFormViewModel> GetForCreateAsync() => new()
    {
        IsActive = true,
        AllProducts = await GetProductSelectItemsAsync()
    };

    public async Task<OfferFormViewModel?> GetForEditAsync(Guid id)
    {
        var offer = await _offerRepository.GetByIdWithItemsAsync(id);
        if (offer == null) return null;

        return new OfferFormViewModel
        {
            Id = offer.Id,
            Title = offer.Title,
            Description = offer.Description,
            IsActive = offer.IsActive,
            SelectedProductIds = offer.Items.Select(i => i.ProductId).ToList(),
            SpecialPrices = offer.Items.ToDictionary(i => i.ProductId, i => i.SpecialPrice),
            AllProducts = await GetProductSelectItemsAsync()
        };
    }

    public async Task<Guid> CreateAsync(OfferFormViewModel model)
    {
        if (model.SelectedProductIds.Count == 0)
            throw new BusinessException("اختر منتج واحد على الأقل", nameof(model.SelectedProductIds));

        var offer = Offer.Create(model.Title, model.Description);

        foreach (var productId in model.SelectedProductIds)
        {
            var specialPrice = model.SpecialPrices.GetValueOrDefault(productId);
            offer.AddItem(productId, specialPrice);
        }

        if (!model.IsActive)
            offer.Deactivate();

        await _offerRepository.AddAsync(offer);
        await _offerRepository.SaveChangesAsync();

        InvalidateCache();

        return offer.Id;
    }

    public async Task UpdateAsync(OfferFormViewModel model)
    {
        var offer = await _offerRepository.GetByIdWithItemsAsync(model.Id)
            ?? throw new BusinessException("العرض غير موجود", nameof(model.Id));

        if (model.SelectedProductIds.Count == 0)
            throw new BusinessException("اختر منتج واحد على الأقل", nameof(model.SelectedProductIds));

        offer.Update(model.Title, model.Description);

        offer.ReplaceItems(model.SelectedProductIds.Select(id =>
            (id, model.SpecialPrices.GetValueOrDefault(id))));

        foreach (var e in _context.ChangeTracker.Entries<OfferItem>())
        {
            Console.WriteLine($"{e.Entity.Id} => {e.State}");
        }

        if (model.IsActive) offer.Activate(); else offer.Deactivate();

        await _offerRepository.SaveChangesAsync();

        InvalidateCache();
    }

    public async Task DeleteAsync(Guid id)
    {
        var offer = await _offerRepository.GetByIdAsync(id)
            ?? throw new BusinessException("العرض غير موجود", nameof(id));

        await _offerRepository.DeleteAsync(offer);
        await _offerRepository.SaveChangesAsync();

        InvalidateCache();
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        var offer = await _offerRepository.GetByIdAsync(id)
            ?? throw new BusinessException("العرض غير موجود", nameof(id));

        if (offer.IsActive) offer.Deactivate(); else offer.Activate();

        await _offerRepository.UpdateAsync(offer);
        await _offerRepository.SaveChangesAsync();

        InvalidateCache();
    }

    private async Task<List<ProductSelectItem>> GetProductSelectItemsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products
            .Where(p => p.IsAvailable)
            .OrderBy(p => p.Name)
            .Select(p => new ProductSelectItem
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                SaleUnit = p.SaleUnit,
                ImagePath = p.ImagePath
            }).ToList();
    }

    private static OfferViewModel MapToViewModel(Offer offer) => new()
    {
        Id = offer.Id,
        Title = offer.Title,
        Description = offer.Description,
        IsActive = offer.IsActive,
        Items = offer.Items.Select(i => new OfferItemViewModel
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            ProductImagePath = i.Product?.ImagePath,
            SaleUnit = i.Product?.SaleUnit ?? Models.Enums.SaleUnit.Piece,
            OriginalPrice = i.Product?.Price ?? 0,
            SpecialPrice = i.SpecialPrice
        }).ToList()
    };

    private void InvalidateCache()
    {
        _cache.Remove(ActiveOffersCacheKey);
        _catalogChangeTracker.Touch();
    }
}
