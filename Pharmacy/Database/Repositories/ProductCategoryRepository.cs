using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly PharmacyDbContext _context;
    
    public ProductCategoryRepository(PharmacyDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<ProductCategory>> GetAllAsync()
    {
        return await _context.ProductCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddAsync(ProductCategory productCategory)
    {
        await _context.ProductCategories.AddAsync(productCategory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductCategory productCategory)
    {
        _context.ProductCategories.Update(productCategory);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductCategory?> GetByIdAsync(int categoryId)
    {
        return await _context.ProductCategories
            .Include(x => x.Fields)
            .FirstOrDefaultAsync(x => x.Id == categoryId);
    }
    
    public async Task DeleteAsync(ProductCategory productCategory)
    {
        _context.ProductCategories.Remove(productCategory);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> ExistsAsync(int categoryId)
    {
        return await _context.ProductCategories.AnyAsync(x => x.Id == categoryId);
    }

    public async Task<bool> ExistsByNameOrDescriptionAsync(string name, string description)
    {
        return await _context.ProductCategories.AnyAsync(x => x.Name.ToLower() == name.ToLower() || x.Description.ToLower() == description.ToLower());
    }
    
    public async Task<IEnumerable<ProductCategoryField>> GetCategoryFieldsAsync(int categoryId)
    {
        return await _context.ProductCategoryFields
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }
}