using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class OfferService : IOfferService
{
    private const string ActiveOffersCacheKey = "offers:active-list";

    private readonly IOfferRepository _offerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _cache;
    private readonly ICatalogChangeTracker _catalogChangeTracker;
    private readonly IPushNotificationService _pushService;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly ILogger<OfferService> _logger;

    public OfferService(
        IOfferRepository offerRepository,
        IProductRepository productRepository,
        IMemoryCache cache,
        ICatalogChangeTracker catalogChangeTracker,
        IPushNotificationService pushService,
        IRealtimeNotifier realtimeNotifier,
        ILogger<OfferService> logger)
    {
        _offerRepository = offerRepository;
        _productRepository = productRepository;
        _cache = cache;
        _catalogChangeTracker = catalogChangeTracker;
        _pushService = pushService;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
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

        if (offer.IsActive)
        {
            await SafeNotifyAsync(() => _pushService.SendToAllCustomersAsync(
                "عرض جديد 🎁",
                $"عرض جديد: {offer.Title}",
                "/Offers"));

            await SafeNotifyAsync(() => _realtimeNotifier.NotifyAllCustomersAsync(
                "عرض جديد 🎁",
                $"عرض جديد: {offer.Title}",
                "/Offers"));
        }

        return offer.Id;
    }

    public async Task UpdateAsync(OfferFormViewModel model)
    {
        var offer = await _offerRepository.GetByIdWithItemsAsync(model.Id)
            ?? throw new BusinessException("العرض غير موجود", nameof(model.Id));

        if (model.SelectedProductIds.Count == 0)
            throw new BusinessException("اختر منتج واحد على الأقل", nameof(model.SelectedProductIds));

        offer.Update(model.Title, model.Description);

        var newItems = offer.ReplaceItems(model.SelectedProductIds.Select(id =>
            (id, model.SpecialPrices.GetValueOrDefault(id))));

        // EF can't reliably auto-detect these as "new" (see MarkItemsAsAdded docs) since they
        // only surface via collection fixup on an already-tracked Offer — mark them explicitly.
        _offerRepository.MarkItemsAsAdded(newItems);

        var wasActive = offer.IsActive;
        if (model.IsActive) offer.Activate(); else offer.Deactivate();

        try
        {
            await _offerRepository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else changed or deleted this offer between the moment it was loaded for
            // editing and this save. Surface a friendly message instead of a raw 500 page.
            throw new BusinessException(
                "تم تعديل هذا العرض من مكان آخر في نفس الوقت، من فضلك أعد تحميل الصفحة وحاول مرة أخرى.",
                nameof(model.Id));
        }

        InvalidateCache();

        // Only notify on the inactive -> active transition, not on every edit of an already-live offer
        if (!wasActive && offer.IsActive)
        {
            await SafeNotifyAsync(() => _pushService.SendToAllCustomersAsync(
                "عرض جديد 🎁",
                $"عرض جديد: {offer.Title}",
                "/Offers"));

            await SafeNotifyAsync(() => _realtimeNotifier.NotifyAllCustomersAsync(
                "عرض جديد 🎁",
                $"عرض جديد: {offer.Title}",
                "/Offers"));
        }
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

        var wasActive = offer.IsActive;
        if (offer.IsActive) offer.Deactivate(); else offer.Activate();

        await _offerRepository.UpdateAsync(offer);
        await _offerRepository.SaveChangesAsync();

        InvalidateCache();

        if (!wasActive && offer.IsActive)
        {
            await SafeNotifyAsync(() => _pushService.SendToAllCustomersAsync(
                "عرض جديد 🎁",
                $"عرض جديد: {offer.Title}",
                "/Offers"));

            await SafeNotifyAsync(() => _realtimeNotifier.NotifyAllCustomersAsync(
                "عرض جديد 🎁",
                $"عرض جديد: {offer.Title}",
                "/Offers"));
        }
    }

    // Push notifications are a nice-to-have layered on top of the real business action. A push
    // failure (bad config, network blip, expired subscription...) must never break offer save/create.
    private async Task SafeNotifyAsync(Func<Task> notify)
    {
        try
        {
            await notify();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send a push notification (offer flow was not affected).");
        }
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
                SaleUnitName = p.SaleUnit?.Name ?? string.Empty,
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
            SaleUnitName = i.Product?.SaleUnit?.Name ?? string.Empty,
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
