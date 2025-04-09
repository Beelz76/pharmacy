using Pharmacy.Database.Entities;
using Pharmacy.Shared.Result;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IManufacturerRepository
{
    Task<IEnumerable<Manufacturer>> GetAllAsync();
    Task<Manufacturer?> GetByIdAsync(int id);
    Task<Manufacturer?> GetByNameAsync(string name, int? excludeId = null);
    Task AddAsync(Manufacturer manufacturer);
    Task UpdateAsync(Manufacturer manufacturer);
    Task DeleteAsync(Manufacturer manufacturer);
    Task<Result> ExecuteInTransactionAsync(Func<Task<Result>> action);
}