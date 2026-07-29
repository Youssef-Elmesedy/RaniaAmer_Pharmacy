using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Generic;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC.Repository.Implementations;

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

    public async Task<IReadOnlyList<Product>> GetAllIncludeCategory() => await _context.Products.Include(p => p.Category).AsNoTracking().ToListAsync();

    public Task<Product?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<Product>> GetAllAsync() => _read.GetAllAsync();

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
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

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
            .AsNoTracking()
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

    public async Task<IReadOnlyList<Product>> GetOffersAsync(int take) =>
        await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .Where(p => p.DiscountPercentage > 0 && p.IsAvailable)
            .OrderByDescending(p => p.DiscountPercentage)
            .Take(take)
            .ToListAsync();
}
