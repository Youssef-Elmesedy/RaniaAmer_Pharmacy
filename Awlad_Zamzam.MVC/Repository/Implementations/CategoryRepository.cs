using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Generic;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC.Repository.Implementations;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<Category> _read;
    private readonly WriteRepository<Category> _write;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<Category>(context);
        _write = new WriteRepository<Category>(context);
    }

    public Task<Category?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<Category>> GetAllAsync() => _read.GetAllAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Category, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Category, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<Category> Query() => _read.Query();

    public Task AddAsync(Category entity) => _write.AddAsync(entity);

    public Task UpdateAsync(Category entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(Category entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null) =>
        _context.Categories.AnyAsync(c =>
            c.NormalizedName == normalizedName && (excludeId == null || c.Id != excludeId));

    public async Task<IReadOnlyList<Category>> GetAllWithProductsAsync() =>
        await _context.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
}
