using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Cart;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _repository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<Result<CartDto>> GetByUserAsync(int userId)
    {
        var items = (await _repository.GetByUserAsync(userId)).ToList();
        var total = items.Sum(x => x.TotalPrice);
        return Result.Success(new CartDto(items, total));
    }

    public async Task<Result<int>> GetCountAsync(int userId)
    {
        var count = await _repository.CountItemsByUserAsync(userId);
        return Result.Success(count);
    }
    
    public async Task<Result> AddToCartAsync(int userId, int productId)
    {
        var product = await _productRepository.GetByIdWithRelationsAsync(productId);
        if (product is null || product.IsGloballyDisabled)
        {
            return Result.Failure(Error.Failure("Товар недоступен для добавления в корзину"));
        }
        
        var existing = await _repository.GetAsync(userId, productId);
        if (existing is null)
        {
            await _repository.AddAsync(new CartItem
            {
                UserId = userId, 
                ProductId = productId, 
                Quantity = 1
            });
        }
        else
        {
            existing.Quantity += 1;
            await _repository.UpdateAsync(existing);
        }
        return Result.Success();
    }

    public async Task<Result> AddRangeAsync(int userId, IEnumerable<CartItemQuantityDto> items)
    {
        foreach (var item in items)
        {
            if (item.Quantity < 1) continue;

            var product = await _productRepository.GetByIdWithRelationsAsync(item.ProductId);
            if (product is null || product.IsGloballyDisabled)
                continue;

            var existing = await _repository.GetAsync(userId, item.ProductId);
            if (existing is null)
            {
                await _repository.AddAsync(new CartItem
                {
                    UserId = userId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }
            else
            {
                existing.Quantity += item.Quantity;
                await _repository.UpdateAsync(existing);
            }
        }

        return Result.Success();
    }
    
    public async Task<Result> SetQuantityAsync(int userId, int productId, int quantity)
    {
        var product = await _productRepository.GetByIdWithRelationsAsync(productId);
        if (product is null || product.IsGloballyDisabled)
        {
            return Result.Failure(Error.Failure("Товар недоступен"));
        }
        
        var existing = await _repository.GetAsync(userId, productId);
        if (existing is null && quantity > 0)
        {
            await _repository.AddAsync(new CartItem
            {
                UserId = userId, 
                ProductId = productId, 
                Quantity = quantity
            });
        }
        else if (existing is not null)
        {
            existing.Quantity = quantity;
            await _repository.UpdateAsync(existing);
        }
        return Result.Success();
    }

    public async Task<Result> RemoveFromCartAsync(int userId, int productId)
    {
        var existing = await _repository.GetAsync(userId, productId);
        if (existing is null)
        {
            return Result.Failure(Error.Failure("Товар не найден в корзине"));
        }

        if (existing.Quantity <= 1)
        {
            await _repository.RemoveAsync(existing);
        }
        else
        {
            existing.Quantity -= 1;
            await _repository.UpdateAsync(existing);
        }
        return Result.Success();
    }

    public async Task<Result> RemoveFromCartCompletelyAsync(int userId, int productId)
    {
        var existing = await _repository.GetAsync(userId, productId);
        if (existing is null)
        {
            return Result.Failure(Error.Failure("Товар не найден в корзине"));
        }
        
        await _repository.RemoveAsync(existing);
        return Result.Success();
    }

    public async Task<Result> ClearCartAsync(int userId)
    {
        var items = await _repository.GetRawUserCartAsync(userId);
        if (items.Any())
        {
            await _repository.RemoveRangeAsync(items);
        }
        return Result.Success();
    }
}