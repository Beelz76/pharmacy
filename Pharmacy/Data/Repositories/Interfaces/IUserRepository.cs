using Pharmacy.Models.Entities;
using Pharmacy.Models.Result;

namespace Pharmacy.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<Result> ExecuteInTransactionAsync(Func<Task<Result>> action);
}