using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.ExternalServices;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Cart;

namespace Pharmacy.Database.Repositories;

public class CartRepository : ICartRepository
{
    private readonly PharmacyDbContext _context;
    private readonly IStorageProvider _storage;
    
    public CartRepository(PharmacyDbContext context, IStorageProvider storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<List<CartItemDto>> GetByUserAsync(int userId)
    {
        return await _context.CartItems
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(c => new CartItemDto(
                c.ProductId,
                c.Product.Name,
                c.Product.Description,
                c.Product.Manufacturer.Name,
                c.Product.Manufacturer.Country,
                c.Quantity,
                c.Product.Price,
                c.Quantity * c.Product.Price,
                c.Product.Images.OrderBy(x => x.Id).Select(x => _storage.GetPublicUrl(x.Url)).FirstOrDefault(),
                !c.Product.IsGloballyDisabled,
                c.Product.IsPrescriptionRequired
                ))
            .ToListAsync();
    }

    public async Task<int> CountItemsByUserAsync(int userId)
    {
        return await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .SumAsync(ci => ci.Quantity);
    }
    
    public async Task<ICollection<CartItem>> GetRawUserCartAsync(int userId)
    {
        return await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<CartItem?> GetAsync(int userId, int productId)
    {
        return await _context.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
    }

    public async Task AddAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CartItem item)
    {
        _context.CartItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(CartItem item)
    {
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<CartItem> items)
    {
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}