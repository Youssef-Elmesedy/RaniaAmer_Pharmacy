using Awlad_Zamzam.MVC.Models.Entities;

namespace Awlad_Zamzam.MVC.Repository.Interfaces;

public interface ICustomerRepository : IReadRepository<Customer>, IWriteRepository<Customer>
{
    Task<bool> ExistsByPhoneAsync(string phoneNumber);

    Task<Customer?> GetByPhoneAsync(string phoneNumber);

    Task<Customer?> GetByNameAndPhoneAsync(string name, string phone);
}
