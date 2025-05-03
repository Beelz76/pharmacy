using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IFavoritesService
{
    Task<Result<IEnumerable<FavoriteItemDto>>> GetByUserAsync(int userId);
    Task<Result> AddAsync(int userId, int productId);
    Task<Result> RemoveAsync(int userId, int productId);
    Task<Result<bool>> ExistsAsync(int userId, int productId);
    Task<Result> ClearAsync(int userId);
}