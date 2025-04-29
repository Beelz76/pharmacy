using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IProductCategoryRepository
{
    Task<IEnumerable<ProductCategory>> GetAllAsync();
    Task AddAsync(ProductCategory productCategory);
    Task<ProductCategory?> GetByIdAsync(int categoryId);
    Task UpdateAsync(ProductCategory productCategory);
    Task DeleteAsync(ProductCategory productCategory);
    Task<bool> ExistsAsync(int categoryId);
    Task<bool> ExistsByNameOrDescriptionAsync(string name, string description);
    Task<IEnumerable<ProductCategoryField>> GetCategoryFieldsAsync(int categoryId);
}