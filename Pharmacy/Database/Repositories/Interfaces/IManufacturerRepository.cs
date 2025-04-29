using Pharmacy.Database.Entities;
using Pharmacy.Shared.Result;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IManufacturerRepository
{
    Task<IEnumerable<Manufacturer>> GetAllAsync();
    Task<Manufacturer?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsAsync(int manufacturerId);
    Task AddAsync(Manufacturer manufacturer);
    Task UpdateAsync(Manufacturer manufacturer);
    Task DeleteAsync(Manufacturer manufacturer);
    Task<Result> ExecuteInTransactionAsync(Func<Task<Result>> action);
}