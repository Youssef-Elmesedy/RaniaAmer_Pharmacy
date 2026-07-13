using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Generic;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC.Repository.Implementations;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ReadRepository<Customer> _read;
    private readonly WriteRepository<Customer> _write;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
        _read = new ReadRepository<Customer>(context);
        _write = new WriteRepository<Customer>(context);
    }

    public Task<Customer?> GetByIdAsync(Guid id) => _read.GetByIdAsync(id);

    public Task<IReadOnlyList<Customer>> GetAllAsync() => _read.GetAllAsync();

    public Task<bool> ExistsAsync(Guid id) => _read.ExistsAsync(id);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate) =>
        _read.AnyAsync(predicate);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Customer, bool>>? predicate = null) =>
        _read.CountAsync(predicate);

    public IQueryable<Customer> Query() => _read.Query();

    public Task AddAsync(Customer entity) => _write.AddAsync(entity);

    public Task UpdateAsync(Customer entity) => _write.UpdateAsync(entity);

    public Task DeleteAsync(Customer entity) => _write.DeleteAsync(entity);

    public Task<int> SaveChangesAsync() => _write.SaveChangesAsync();

    public Task<bool> ExistsByPhoneAsync(string phoneNumber) =>
        _context.Customers.AnyAsync(c => c.PhoneNumber == phoneNumber);

    public Task<Customer?> GetByPhoneAsync(string phoneNumber) =>
        _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
}
