using Pharmacy.Database.Entities;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface ICartRepository
{
    Task<List<CartItemDto>> GetByUserAsync(int userId);
    Task<int> CountItemsByUserAsync(int userId);
    Task<ICollection<CartItem>> GetRawUserCartAsync(int userId);
    Task<CartItem?> GetAsync(int userId, int productId);
    Task AddAsync(CartItem item);
    Task UpdateAsync(CartItem item);
    Task RemoveAsync(CartItem item);
    Task RemoveRangeAsync(IEnumerable<CartItem> items);
}