using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Repository.Generic;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Repository.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<Product> _read;
    private readonly WriteRepository<Product> _write;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<Product>(context);
        _write = new WriteRepository<Product>(context);
    }

    public Task<Product?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    // NOTE: overrides the generic pass-through (was `_read.GetAllAsync()`, which doesn't include
    // Category) — every admin product list view displays/searches the category name, so it must
    // be eager-loaded here.
    public async Task<IReadOnlyList<Product>> GetAllAsync() =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .AsNoTracking()
            .ToListAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Product, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<Product> Query() => _read.Query();

    public Task AddAsync(Product entity) => _write.AddAsync(entity);

    public Task UpdateAsync(Product entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(Product entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public async Task<Product?> GetByIdWithCategoryAsync(Guid id) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .Include(p => p.UnitOptions).ThenInclude(o => o.SaleUnit)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    // Read-only lookup used where a product's sale unit name is needed for display/snapshot
    // purposes (cart, admin credit-order form) — NOT used anywhere the entity is later
    // updated, since loaded navigation properties would otherwise get needlessly re-saved.
    public async Task<Product?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .Include(p => p.UnitOptions).ThenInclude(o => o.SaleUnit)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    // TRACKED (no AsNoTracking) — used only for the admin edit flow, where the product and its
    // unit options need to stay attached so ReplaceUnitOptions()'s changes are picked up by
    // SaveChanges without the "detached Update() marks whole graph Modified" risk.
    public async Task<Product?> GetByIdWithUnitOptionsAsync(Guid id) =>
        await _context.Products
            .Include(p => p.UnitOptions)
            .FirstOrDefaultAsync(p => p.Id == id);

    // Batched version of GetByIdWithDetailsAsync — one round-trip for many products instead
    // of one per product (avoids N+1 when loading a cart or a multi-item admin order form).
    public async Task<IReadOnlyList<Product>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return Array.Empty<Product>();

        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .Include(p => p.UnitOptions).ThenInclude(o => o.SaleUnit)
            .AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();
    }

    public void MarkUnitOptionsAsAdded(IEnumerable<ProductUnitOption> options)
    {
        foreach (var option in options)
        {
            var entry = _context.Entry(option);
            if (entry.State != EntityState.Added)
                entry.State = EntityState.Added;
        }
    }

    public Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null) =>
        _context.Products.AnyAsync(p =>
            p.NormalizedName == normalizedName && (excludeId == null || p.Id != excludeId));

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId,
        string? searchTerm,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .AsNoTracking()
            .Where(p => p.IsAvailable)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim().ToUpperInvariant();
            query = query.Where(p => p.NormalizedName.Contains(normalizedSearch));
        }

        query = sortOrder switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedForAdminAsync(
        string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim().ToUpperInvariant();
            query = query.Where(p =>
                p.NormalizedName.Contains(normalizedSearch) ||
                (p.Category != null && p.Category.NormalizedName.Contains(normalizedSearch)));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Product>> GetOffersAsync(int take) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .AsNoTracking()
            .Where(p => p.DiscountPercentage > 0 && p.IsAvailable)
            .OrderByDescending(p => p.DiscountPercentage)
            .Take(take)
            .ToListAsync();

    public async Task<IReadOnlyList<Product>> GetAllIncludeCategory()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .AsNoTracking()
            .ToListAsync();

        return products;
    }
}
