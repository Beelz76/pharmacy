using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.PharmacyProducts;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class PharmacyProductService : IPharmacyProductService
{
    private readonly IPharmacyProductRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PharmacyProductService(IPharmacyProductRepository repository, IProductRepository productRepository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _productRepository = productRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IEnumerable<PharmacyProductDto>>> GetByPharmacyAsync(int pharmacyId)
    {
        var list = await _repository.GetByPharmacyIdAsync(pharmacyId);
        var result = list.Select(x => new PharmacyProductDto(
            x.ProductId,
            x.Product.Name,
            x.StockQuantity,
            x.LocalPrice ?? x.Product.Price,
            !x.Product.IsGloballyDisabled && x.IsAvailable));

        return Result.Success(result);
    }

    public async Task<Result<CreatedDto>> AddAsync(int pharmacyId, AddPharmacyProductRequest request)
    {
        var exists = await _repository.ExistsAsync(pharmacyId, request.ProductId);
        if (exists)
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Такой товар уже добавлен в аптеку"));
        }

        var product = await _productRepository.GetByIdWithRelationsAsync(request.ProductId);
        if (product == null)
        {
            return Result.Failure<CreatedDto>(Error.NotFound("Товар не найден"));
        }

        var pharmacyProduct = new PharmacyProduct
        {
            PharmacyId = pharmacyId,
            ProductId = request.ProductId,
            StockQuantity = request.StockQuantity,
            LocalPrice = request.Price ?? product.Price,
            IsAvailable = request.IsAvailable,
            LastRestockedAt = _dateTimeProvider.UtcNow
        };

        await _repository.AddAsync(pharmacyProduct);
        return Result.Success(new CreatedDto(pharmacyProduct.Id));
    }
    
    public async Task<Result> UpdateAsync(int pharmacyId, int productId, UpdatePharmacyProductRequest request)
    {
        var pharmacyProduct = await _repository.GetAsync(pharmacyId, productId);
        if (pharmacyProduct == null)
        {
            return Result.Failure(Error.NotFound("Товар в аптеке не найден"));
        }

        var product = await _productRepository.GetByIdWithRelationsAsync(productId);
        if (product == null)
        {
            return Result.Failure(Error.NotFound("Товар не найден"));
        }

        if (pharmacyProduct.StockQuantity != request.StockQuantity)
        {
            pharmacyProduct.LastRestockedAt = _dateTimeProvider.UtcNow;
        }
        pharmacyProduct.StockQuantity = request.StockQuantity;
        pharmacyProduct.LocalPrice = request.Price ?? product.Price;
        pharmacyProduct.IsAvailable = request.IsAvailable;

        await _repository.UpdateAsync(pharmacyProduct);
        return Result.Success();
    }

    
    public async Task<Result> DeleteAsync(int pharmacyId, int productId)
    {
        var entity = await _repository.GetAsync(pharmacyId, productId);
        if (entity == null)
        {
            return Result.Failure(Error.NotFound("Товар в аптеке не найден"));
        }

        await _repository.DeleteAsync(entity);
        return Result.Success();
    }
    
    public async Task<Result<List<PharmacyProductDto>>> ValidateOrAddProductsAsync(int pharmacyId, List<(int productId, int quantity)> items)
    {
        var now = _dateTimeProvider.UtcNow;
        var result = new List<PharmacyProductDto>();

        var existingProducts = await _repository.GetByPharmacyIdAsync(pharmacyId);
        var existingDict = existingProducts.ToDictionary(x => x.ProductId);

        foreach (var (productId, quantity) in items)
        {
            if (existingDict.TryGetValue(productId, out var pharmacyProduct))
            {
                if (pharmacyProduct.Product.IsGloballyDisabled)
                    return Result.Failure<List<PharmacyProductDto>>(Error.Failure($"Товар {productId} отключен глобально"));

                if (!pharmacyProduct.IsAvailable)
                    return Result.Failure<List<PharmacyProductDto>>(Error.Failure($"Товар {productId} недоступен в аптеке"));

                if (pharmacyProduct.StockQuantity < quantity)
                    return Result.Failure<List<PharmacyProductDto>>(Error.Failure($"Недостаточно товара {productId} на складе аптеки"));

                result.Add(new PharmacyProductDto(
                    pharmacyProduct.ProductId,
                    pharmacyProduct.Product.Name,
                    pharmacyProduct.StockQuantity,
                    pharmacyProduct.LocalPrice ?? pharmacyProduct.Product.Price,
                    true));
            }
            else
            {
                var product = await _productRepository.GetByIdWithRelationsAsync(productId);
                if (product == null || product.IsGloballyDisabled)
                    return Result.Failure<List<PharmacyProductDto>>(Error.Failure($"Товар {productId} недоступен"));

                var newPharmacyProduct = new PharmacyProduct
                {
                    PharmacyId = pharmacyId,
                    ProductId = productId,
                    StockQuantity = quantity,
                    LocalPrice = product.Price,
                    IsAvailable = true,
                    LastRestockedAt = now
                };

                await _repository.AddAsync(newPharmacyProduct);

                result.Add(new PharmacyProductDto(
                    newPharmacyProduct.ProductId,
                    product.Name,
                    quantity,
                    newPharmacyProduct.LocalPrice!.Value,
                    true));
            }
        }

        return Result.Success(result);
    }
}