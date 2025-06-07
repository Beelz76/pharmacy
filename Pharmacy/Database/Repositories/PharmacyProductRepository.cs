using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class PharmacyProductRepository : IPharmacyProductRepository
{
    private readonly PharmacyDbContext _context;

    public PharmacyProductRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<List<PharmacyProduct>> GetByPharmacyIdAsync(int pharmacyId)
    {
        return await _context.PharmacyProducts
            .Include(x => x.Product)
            .Where(x => x.PharmacyId == pharmacyId)
            .ToListAsync();
    }

    public async Task<PharmacyProduct?> GetAsync(int pharmacyId, int productId)
    {
        return await _context.PharmacyProducts.FirstOrDefaultAsync(x => x.PharmacyId == pharmacyId && x.ProductId == productId);
    }
    
    public async Task<bool> ExistsAsync(int pharmacyId, int productId)
    {
        return await _context.PharmacyProducts.AnyAsync(x => x.PharmacyId == pharmacyId && x.ProductId == productId);
    }

    public async Task AddAsync(PharmacyProduct pharmacyProduct)
    {
        _context.PharmacyProducts.Add(pharmacyProduct);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PharmacyProduct pharmacyProduct)
    {
        _context.PharmacyProducts.Update(pharmacyProduct);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(PharmacyProduct pharmacyProduct)
    {
        _context.PharmacyProducts.Remove(pharmacyProduct);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateStockQuantityAsync(int pharmacyId, int productId, int stockQuantity)
    {
        await _context.PharmacyProducts
            .Where(x => x.PharmacyId == pharmacyId && x.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.StockQuantity, stockQuantity));
    }
    
}