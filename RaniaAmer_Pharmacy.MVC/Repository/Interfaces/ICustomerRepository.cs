using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Repository.Interfaces;

public interface ICustomerRepository : IReadRepository<Customer>, IWriteRepository<Customer>
{
    Task<bool> ExistsByPhoneAsync(string phoneNumber);

    Task<Customer?> GetByPhoneAsync(string phoneNumber);

    Task<Customer?> GetByNameAndPhoneAsync(string name, string phone);
}
