using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Favorites;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class FavoritesService : IFavoritesService
{
    private readonly IFavoritesRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IStorageProvider _storage;

    public FavoritesService(IFavoritesRepository repository, IProductRepository productRepository, ICartRepository cartRepository, IStorageProvider storage)
    {
        _repository = repository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _storage = storage;
    }

    public async Task<Result<IEnumerable<FavoriteItemDto>>> GetByUserAsync(int userId)
    {
        var favorites = await _repository.GetByUserAsync(userId);
        var cartItems = (await _cartRepository.GetRawUserCartAsync(userId)).ToDictionary(c => c.ProductId, c => c.Quantity);

        var result = favorites.Select(x => new FavoriteItemDto(
            x.ProductId,
            x.Name,
            x.Description,
            x.ManufacturerName,
            x.ManufacturerCountry,
            x.Price,
            !string.IsNullOrWhiteSpace(x.ImageUrl) && x.ImageUrl.StartsWith("http") ? x.ImageUrl : _storage.GetPublicUrl(x.ImageUrl),
            x.IsAvailable,
            cartItems.TryGetValue(x.ProductId, out int qty) ? qty : 0
        ));
        
        return Result.Success(result);
    }
    
    public async Task<Result<int>> GetCountAsync(int userId)
    {
        var count = await _repository.CountByUserAsync(userId);
        return Result.Success(count);
    }

    public async Task<Result> AddAsync(int userId, int productId)
    {
        if (await _repository.ExistsAsync(userId, productId))
        {
            return Result.Success();
        }

        var product = await _productRepository.GetByIdWithRelationsAsync(productId);
        if (product is null)
        {
            return Result.Failure(Error.NotFound("Товар недоступен"));
        }

        await _repository.AddAsync(new FavoriteItem
        {
            UserId = userId, 
            ProductId = productId
        });
        return Result.Success();
    }

    public async Task<Result> AddRangeAsync(int userId, IEnumerable<int> productIds)
    {
        foreach (var productId in productIds.Distinct())
        {
            if (await _repository.ExistsAsync(userId, productId))
                continue;

            var product = await _productRepository.GetByIdWithRelationsAsync(productId);
            if (product is null)
                continue;

            await _repository.AddAsync(new FavoriteItem
            {
                UserId = userId,
                ProductId = productId
            });
        }

        return Result.Success();
    }
    
    public async Task<Result> RemoveAsync(int userId, int productId)
    {
        var toRemove = await _repository.GetAsync(userId, productId);
        if (toRemove is null)
        {
            return Result.Failure(Error.NotFound("Товар не найден в избранном"));
        }

        await _repository.RemoveAsync(toRemove);
        return Result.Success();
    }

    public async Task<Result<bool>> ExistsAsync(int userId, int productId)
    {
        var exists = await _repository.ExistsAsync(userId, productId);
        return Result.Success(exists);
    }

    public async Task<Result> ClearAsync(int userId)
    {
        var items = await _repository.GetRawUserFavoritesAsync(userId);
        if (items.Any())
        {
            await _repository.RemoveRangeAsync(items);
        }
        return Result.Success();
    }
}