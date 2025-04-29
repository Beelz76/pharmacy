using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetDetailsByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> HasMissingFieldAsync(int categoryId, string fieldKey);
    Task<bool> HasFieldUsedAsync(int categoryId, string fieldKey);
    Task<List<string>> GetSearchSuggestionsAsync(string query);
    IQueryable<Product> QueryWithProperties();
}