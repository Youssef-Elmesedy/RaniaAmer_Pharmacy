using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Repository.Implementations;

public class SiteSettingsRepository : ISiteSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SiteSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // There is always exactly one row; AsNoTracking since callers that mutate it re-attach
    // explicitly (kept consistent with the rest of the repositories in this project).
    public Task<SiteSettings?> GetAsync() =>
        _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();

    public async Task AddAsync(SiteSettings settings) =>
        await _context.SiteSettings.AddAsync(settings);

    // Safe to re-attach and mark the whole entity Modified: SiteSettings has no navigation
    // properties/children, so there's no risk of unintended cascading updates.
    public Task UpdateAsync(SiteSettings settings)
    {
        _context.SiteSettings.Update(settings);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
