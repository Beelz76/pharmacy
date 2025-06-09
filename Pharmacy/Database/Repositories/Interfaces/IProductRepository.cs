using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdWithRelationsAsync(int id, bool includeProperties = false, bool includeImages = false, bool includeCategory = false, bool includeManufacturer = false);
    Task<bool> ExistsAsync(int productId = 0, int categoryId = 0, string? name = null, string? description = null, int? excludeId = null);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> HasMissingFieldAsync(int categoryId, string fieldKey);
    Task<bool> HasFieldUsedAsync(int categoryId, string fieldKey);
    Task<List<string>> GetSearchSuggestionsAsync(string query);
    Task<Product?> GetBySkuAsync(string sku);
    Task<string> GetLastSkuAsync();
    IQueryable<Product> Query();
    IQueryable<Product> QueryWithProperties();
}