using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface ICartService
{
    Task<Result<CartDto>> GetByUserAsync(int userId);
    Task<Result<int>> GetCountAsync(int userId);
    Task<Result> AddToCartAsync(int userId, int productId);
    Task<Result> AddRangeAsync(int userId, IEnumerable<CartItemQuantityDto> items);
    Task<Result> SetQuantityAsync(int userId, int productId, int quantity);
    Task<Result> RemoveFromCartAsync(int userId, int productId);
    Task<Result> RemoveFromCartCompletelyAsync(int userId, int productId);
    Task<Result> ClearCartAsync(int userId);
}