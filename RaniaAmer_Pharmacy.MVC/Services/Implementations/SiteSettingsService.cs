using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

// Cached because this is read on every public page (footer + contact page).
public class SiteSettingsService : ISiteSettingsService
{
    private const string CacheKey = "site-settings";

    private readonly ISiteSettingsRepository _repository;
    private readonly IMemoryCache _cache;

    public SiteSettingsService(ISiteSettingsRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<StoreSettingsViewModel> GetAsync()
    {
        if (_cache.TryGetValue(CacheKey, out StoreSettingsViewModel? cached) && cached != null)
            return cached;

        var settings = await _repository.GetAsync();

        if (settings == null)
        {
            settings = SiteSettings.CreateDefault();
            await _repository.AddAsync(settings);
            await _repository.SaveChangesAsync();
        }

        var model = MapToViewModel(settings);
        _cache.Set(CacheKey, model, TimeSpan.FromMinutes(30));

        return model;
    }

    public async Task UpdateAsync(StoreSettingsViewModel model)
    {
        var settings = await _repository.GetAsync();
        var isNew = settings == null;

        settings ??= SiteSettings.CreateDefault();

        settings.Update(
            model.PharmacyName,
            model.WhatsAppNumber,
            model.FacebookUrl,
            model.InstagramUrl);

        if (isNew)
            await _repository.AddAsync(settings);
        else
            await _repository.UpdateAsync(settings);

        await _repository.SaveChangesAsync();

        _cache.Remove(CacheKey);
    }

    private static StoreSettingsViewModel MapToViewModel(SiteSettings s) => new()
    {
        PharmacyName = s.PharmacyName,
        WhatsAppNumber = s.WhatsAppNumber,
        FacebookUrl = s.FacebookUrl,
        InstagramUrl = s.InstagramUrl
    };
}
