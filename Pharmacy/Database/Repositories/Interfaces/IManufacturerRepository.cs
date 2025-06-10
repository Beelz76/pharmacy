using Pharmacy.Database.Entities;
using Pharmacy.Shared.Result;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IManufacturerRepository
{
    Task<IEnumerable<Manufacturer>> GetAllAsync();
    Task<Manufacturer?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int manufacturerId = 0, string? name = null, int? excludeId = null);
    Task AddAsync(Manufacturer manufacturer);
    Task UpdateAsync(Manufacturer manufacturer);
    Task DeleteAsync(Manufacturer manufacturer);
}