using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Repository.Generic;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Repository.Implementations;

public class OfferRepository : IOfferRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<Offer> _read;
    private readonly WriteRepository<Offer> _write;

    public OfferRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<Offer>(context);
        _write = new WriteRepository<Offer>(context);
    }

    public Task<Offer?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<Offer>> GetAllAsync() => _read.GetAllAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Offer, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Offer, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<Offer> Query() => _read.Query();

    public Task AddAsync(Offer entity) => _write.AddAsync(entity);

    public Task UpdateAsync(Offer entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(Offer entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public async Task<IReadOnlyList<Offer>> GetActiveWithItemsAsync() =>
        await _context.Offers
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.SaleUnit)
            .AsNoTracking()
            .Where(o => o.IsActive)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<Offer?> GetByIdWithItemsAsync(Guid id) =>
        await _context.Offers
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.SaleUnit)
            .FirstOrDefaultAsync(o => o.Id == id);

    public void MarkItemsAsAdded(IEnumerable<OfferItem> items)
    {
        foreach (var item in items)
        {
            var entry = _context.Entry(item);
            if (entry.State != EntityState.Added)
                entry.State = EntityState.Added;
        }
    }
}
