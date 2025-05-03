using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IProductCategoryRepository
{
    Task<IEnumerable<ProductCategory>> GetAllAsync();
    Task<IEnumerable<ProductCategory>> GetSubcategoriesAsync(int parentId);
    Task<ProductCategory?> GetByIdWithRelationsAsync(int categoryId, bool includeFields = false, bool includeParent = false, bool includeSubcategories = false);
    Task<bool> ExistsAsync(int categoryId = 0, string? name = null, string? description = null, int? excludeId = null);
    Task AddAsync(ProductCategory productCategory);
    Task UpdateAsync(ProductCategory productCategory);
    Task DeleteAsync(ProductCategory productCategory);
    Task<List<int>> GetChildrenIdsAsync(int parentCategoryId);
    Task<IEnumerable<ProductCategoryField>> GetCategoryFieldsAsync(int categoryId);
}