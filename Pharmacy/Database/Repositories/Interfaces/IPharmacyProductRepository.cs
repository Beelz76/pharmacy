using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IPharmacyProductRepository
{
    Task<List<PharmacyProduct>> GetByPharmacyIdAsync(int pharmacyId);
    Task<PharmacyProduct?> GetAsync(int pharmacyId, int productId);
    Task<bool> ExistsAsync(int pharmacyId, int productId);
    Task AddAsync(PharmacyProduct pharmacyProduct);
    Task UpdateAsync(PharmacyProduct pharmacyProduct);
    Task DeleteAsync(PharmacyProduct pharmacyProduct);
    Task UpdateStockQuantityAsync(int pharmacyId, int productId, int stockQuantity);
}