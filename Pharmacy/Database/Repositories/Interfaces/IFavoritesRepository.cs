using Pharmacy.Database.Entities;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IFavoritesRepository
{
    Task<IEnumerable<FavoriteProductRawDto>> GetByUserAsync(int userId);
    Task<ICollection<FavoriteItem>> GetRawUserFavoritesAsync(int userId);
    Task<List<int>> GetFavoriteProductIdsAsync(int userId);
    Task<bool> ExistsAsync(int userId, int productId);
    Task<FavoriteItem?> GetAsync(int userId, int productId);
    Task AddAsync(FavoriteItem item);
    Task RemoveAsync(FavoriteItem item);
    Task RemoveRangeAsync(IEnumerable<FavoriteItem> items);
}