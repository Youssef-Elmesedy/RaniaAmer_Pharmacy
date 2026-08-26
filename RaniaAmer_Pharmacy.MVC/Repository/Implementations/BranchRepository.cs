using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Repository.Generic;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Repository.Implementations;

public class BranchRepository : IBranchRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<Branch> _read;
    private readonly WriteRepository<Branch> _write;

    public BranchRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<Branch>(context);
        _write = new WriteRepository<Branch>(context);
    }

    public Task<Branch?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<Branch>> GetAllAsync() => _read.GetAllAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Branch, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Branch, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<Branch> Query() => _read.Query();

    public Task AddAsync(Branch entity) => _write.AddAsync(entity);

    public Task UpdateAsync(Branch entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(Branch entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public async Task<IReadOnlyList<Branch>> GetAllOrderedAsync() =>
        await _context.Branches
            .AsNoTracking()
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();
}
