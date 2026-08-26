using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Repository.Generic;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Repository.Implementations;

public class SaleUnitRepository : ISaleUnitRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<SaleUnit> _read;
    private readonly WriteRepository<SaleUnit> _write;

    public SaleUnitRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<SaleUnit>(context);
        _write = new WriteRepository<SaleUnit>(context);
    }

    public Task<SaleUnit?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<SaleUnit>> GetAllAsync() => _read.GetAllAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<SaleUnit, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<SaleUnit, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<SaleUnit> Query() => _read.Query();

    public Task AddAsync(SaleUnit entity) => _write.AddAsync(entity);

    public Task UpdateAsync(SaleUnit entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(SaleUnit entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public Task<bool> ExistsByNameAsync(string normalizedName, Guid? excludeId = null) =>
        _context.SaleUnits.AnyAsync(u =>
            u.NormalizedName == normalizedName && (excludeId == null || u.Id != excludeId));

    public async Task<IReadOnlyList<SaleUnit>> GetAllWithProductsAsync() =>
        await _context.SaleUnits
            .Include(u => u.Products)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

    public Task<bool> IsUsedAsSubUnitAsync(Guid saleUnitId) =>
        _context.ProductUnitOptions.AnyAsync(o => o.SaleUnitId == saleUnitId);
}
