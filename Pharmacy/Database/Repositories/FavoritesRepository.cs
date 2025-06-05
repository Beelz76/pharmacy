using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Database.Repositories;

public class FavoritesRepository : IFavoritesRepository
{
    private readonly PharmacyDbContext _context;

    public FavoritesRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FavoriteProductRawDto>> GetByUserAsync(int userId)
    {
        return await _context.FavoriteItems
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(f => new FavoriteProductRawDto
            {
                ProductId = f.ProductId,
                Name = f.Product.Name,
                Description = f.Product.Description,
                ManufacturerName = f.Product.Manufacturer.Name,
                ManufacturerCountry = f.Product.Manufacturer.Country,
                Price = f.Product.Price,
                ImageUrl = f.Product.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault(),
                IsAvailable = !f.Product.IsGloballyDisabled,
                IsPrescriptionRequired = f.Product.IsPrescriptionRequired
            })
            .ToListAsync();
    }

    public async Task<int> CountByUserAsync(int userId)
    {
        return await _context.FavoriteItems
            .Where(x => x.UserId == userId)
            .CountAsync();
    }
    
    public async Task<ICollection<FavoriteItem>> GetRawUserFavoritesAsync(int userId)
    {
        return await _context.FavoriteItems
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<List<int>> GetFavoriteProductIdsAsync(int userId)
    {
        return await _context.FavoriteItems
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ProductId)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsAsync(int userId, int productId)
    {
        return await _context.FavoriteItems.AnyAsync(x => x.UserId == userId && x.ProductId == productId);
    }

    public async Task<FavoriteItem?> GetAsync(int userId, int productId)
    {
        return await _context.FavoriteItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
    }
    
    public async Task AddAsync(FavoriteItem item)
    {
        await _context.FavoriteItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(FavoriteItem item)
    {
        _context.FavoriteItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<FavoriteItem> items)
    {
        _context.FavoriteItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}