using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PharmacyDbContext _context;
    
    public ProductRepository(PharmacyDbContext context)
    {
        _context = context;
    }
    
    public async Task<Product?> GetByIdWithRelationsAsync(int id, bool includeProperties = false, bool includeImages = false, bool includeCategory = false, bool includeManufacturer = false)
    {
        var query = _context.Products.AsQueryable();

        if (includeProperties)
            query = query.Include(x => x.Properties);
        
        if (includeImages)
            query = query.Include(x => x.Images);

        if (includeCategory)
            query = query
                .Include(x => x.ProductCategory)
                    .ThenInclude(c => c.Fields)
                .Include(x => x.ProductCategory)
                    .ThenInclude(c => c.ParentCategory)
                        .ThenInclude(pc => pc.Fields);
        
        if (includeManufacturer)
            query = query.Include(x => x.Manufacturer);

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<bool> ExistsAsync(int productId = 0, int categoryId = 0, string? name = null, string? description = null, int? excludeId = null)
    {
        return await _context.Products.AnyAsync(x =>
            ((productId != 0 && x.Id == productId) || 
             (categoryId != 0 && x.CategoryId == categoryId) ||
             (!string.IsNullOrEmpty(name) && x.Name.ToLower() == name.ToLower()) ||
             (!string.IsNullOrEmpty(description) && x.Description.ToLower() == description.ToLower())) && 
            (!excludeId.HasValue || x.Id != excludeId));
    }
    
    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductCategoryField>> GetFieldsByCategoryIdAsync(int categoryId)
    {
        return await _context.ProductCategoryFields
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<bool> HasMissingFieldAsync(int categoryId, string fieldKey)
    {
        return await _context.Products
            .Include(x => x.Properties)
            .Where(x => x.CategoryId == categoryId)
            .AnyAsync(x => !x.Properties.Any(prop => prop.Key == fieldKey && !string.IsNullOrWhiteSpace(prop.Value)));
    }
    
    public async Task<bool> HasFieldUsedAsync(int categoryId, string fieldKey)
    {
        return await _context.Products
            .Where(x => x.CategoryId == categoryId)
            .SelectMany(x => x.Properties)
            .AnyAsync(x => x.Key == fieldKey && !string.IsNullOrWhiteSpace(x.Value));
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string query)
    {
        return await _context.Products
            .Where(p => p.Name.ToLower().Contains(query.ToLower()))
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .Distinct()
            .Take(10)
            .ToListAsync();
    }
    
    public async Task<string> GetLastSkuAsync()
    {
        var lastSku = await _context.Products
            .OrderByDescending(p => p.Id)
            .Select(p => p.Sku)
            .FirstOrDefaultAsync();
        return lastSku ?? "PRD-000000";
    }
    
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Properties)
            .Include(p => p.Images)
            .Include(p => p.ProductCategory)
            .Include(p => p.Manufacturer)
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }
    
    public IQueryable<Product> Query()
    {
        return _context.Products
            .AsNoTracking()
            .AsQueryable();
    }
    
    public IQueryable<Product> QueryWithProperties()
    {
        return _context.Products
            .Include(p => p.Properties)
            .AsNoTracking()
            .AsQueryable();
    }
}