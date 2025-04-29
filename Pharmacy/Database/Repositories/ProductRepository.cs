using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class ProductRepository : BaseRepository, IProductRepository
{
    private readonly PharmacyDbContext _context;
    
    public ProductRepository(PharmacyDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<Product?> GetDetailsByIdAsync(int id)
    {
        return await _context.Products
            .Include(x => x.Properties)
            .Include(p => p.ProductCategory)
            .Include(p => p.Manufacturer)
            //.Include(p => p.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
       return await _context.Products.AnyAsync(x => x.Id == id);
    }
    
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Products.AnyAsync(x => x.Name == name);
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
    
    public IQueryable<Product> QueryWithProperties()
    {
        return _context.Products
            .Include(p => p.Properties)
            .AsNoTracking()
            .AsQueryable();
    }
}