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
            .Include(x => x.Subcategories)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ProductCategory>> GetSubcategoriesAsync(int parentId)
    {
        return await _context.ProductCategories
            .Where(x => x.ParentCategoryId == parentId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<ProductCategory?> GetByIdWithRelationsAsync(int categoryId, bool includeFields = false, bool includeParent = false, bool includeSubcategories = false)
    {
        var query = _context.ProductCategories.AsQueryable();

        if (includeFields)
            query = query.Include(x => x.Fields);
        
        if (includeParent)
            query = query.Include(x => x.ParentCategory);

        if (includeSubcategories)
            query = query.Include(x => x.Subcategories);

        return await query.FirstOrDefaultAsync(x => x.Id == categoryId);
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
    
    public async Task DeleteAsync(ProductCategory productCategory)
    {
        _context.ProductCategories.Remove(productCategory);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<int>> GetChildrenIdsAsync(int parentCategoryId)
    {
        return await _context.ProductCategories
            .Where(x => x.ParentCategoryId == parentCategoryId)
            .Select(x => x.Id)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsAsync(int categoryId = 0, string? name = null, string? description = null, int? excludeId = null)
    {
        return await _context.ProductCategories.AnyAsync(x =>
            ((categoryId != 0 && x.Id == categoryId) ||
            (!string.IsNullOrEmpty(name) && x.Name.ToLower() == name.ToLower()) ||
            (!string.IsNullOrEmpty(description) && x.Description.ToLower() == description.ToLower())) && 
            (!excludeId.HasValue || x.Id != excludeId));
    }
    
    public async Task<IEnumerable<ProductCategoryField>> GetCategoryFieldsAsync(int categoryId)
    {
        return await _context.ProductCategoryFields
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }
}