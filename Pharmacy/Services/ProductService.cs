using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Products;
using Pharmacy.Helpers;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IProductCategoryService _productCategoryService;
    private readonly ICartRepository _cartRepository;
    private readonly IFavoritesRepository _favoritesRepository;
    private readonly IManufacturerService _manufacturerService;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public ProductService(IDateTimeProvider dateTimeProvider, IProductRepository repository, IProductCategoryService productCategoryService, IManufacturerService manufacturerService, ICartRepository cartRepository, IFavoritesRepository favoritesRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _repository = repository;
        _productCategoryService = productCategoryService;
        _manufacturerService = manufacturerService;
        _cartRepository = cartRepository;
        _favoritesRepository = favoritesRepository;
    }
    
    public async Task<Result<CreatedDto>> CreateProductAsync(CreateProductRequest request)
    {
        if (await _repository.ExistsAsync(name: request.Name, description: request.Description))
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Товар с таким названием или описанием уже существует"));
        }
        
        if (!await _productCategoryService.ExistsAsync(request.CategoryId))
        {
            return Result.Failure<CreatedDto>(Error.NotFound("Категория не найдена"));
        }
        
        if (!await _manufacturerService.ExistsAsync(request.ManufacturerId))
        {
            return Result.Failure<CreatedDto>(Error.NotFound("Производитель не найден"));
        }
        
        var categoryFieldsResult = await _productCategoryService.GetAllFieldsIncludingParentAsync(request.CategoryId);
        
        var errors = ValidateProductProperties(request.Properties, categoryFieldsResult.Value.ToList());
        
        if (errors.Any())
        {
            return Result.Failure<CreatedDto>(Error.Failure("Ошибки при создании", errors));
        }
        
        var sku = await GenerateNextSkuAsync();
        
        var product = new Product
        {
            Sku = sku,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            ManufacturerId = request.ManufacturerId,
            Description = request.Description,
            ExpirationDate = request.ExpirationDate,
            IsAvailable = request.IsAvailable,
            IsPrescriptionRequired = request.IsPrescriptionRequired,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            Properties = request.Properties.Select(p => new ProductProperty
            {
                Key = p.Key,
                Value = p.Value
            }).ToList()
        };

        await _repository.AddAsync(product);
        return Result.Success(new CreatedDto(product.Id));
    }
    
    public async Task<Result> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdWithRelationsAsync(id, includeCategory: true, includeImages: true, includeManufacturer: true, includeProperties: true);
        if (product is null)
        {
            return Result.Failure(Error.NotFound("Товар не найден"));
        }

        if (await _repository.ExistsAsync(name: request.Name, description: request.Description, excludeId: product.Id))
        {
            return Result.Failure(Error.Conflict("Товар с таким названием или описанием уже существует"));
        }

        if (!await _productCategoryService.ExistsAsync(request.CategoryId))
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        if (!await _manufacturerService.ExistsAsync(request.ManufacturerId))
        {
            return Result.Failure(Error.NotFound("Производитель не найден"));
        }

        var categoryFieldsResult = await _productCategoryService.GetAllFieldsIncludingParentAsync(request.CategoryId);
        var categoryFields = categoryFieldsResult.Value.ToList();

        var errors = ValidateProductProperties(request.Properties, categoryFields);
        if (errors.Any())
        {
            return Result.Failure<CreatedDto>(Error.Failure("Ошибки при обновлении", errors));
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.ManufacturerId = request.ManufacturerId;
        product.Description = request.Description;
        product.ExpirationDate = request.ExpirationDate;
        product.IsAvailable = request.IsAvailable;
        product.IsPrescriptionRequired = request.IsPrescriptionRequired;
        product.UpdatedAt = _dateTimeProvider.UtcNow;

        var existingProperties = product.Properties.ToDictionary(p => p.Key, p => p, StringComparer.OrdinalIgnoreCase);
        
        foreach (var propDto in request.Properties)
        {
            if (existingProperties.TryGetValue(propDto.Key, out var existingProp))
            {
                existingProp.Value = propDto.Value;
            }
            else
            {
                product.Properties.Add(new ProductProperty
                {
                    Key = propDto.Key,
                    Value = propDto.Value
                });
            }
        }
        
        var requestKeys = request.Properties.Select(p => p.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allowedFields = categoryFields.ToDictionary(f => f.Key, f => f, StringComparer.OrdinalIgnoreCase);
        
        var propertiesToRemove = product.Properties
            .Where(p => 
                !requestKeys.Contains(p.Key) &&
                allowedFields.TryGetValue(p.Key, out var field) &&
                !field.IsRequired)
            .ToList();

        foreach (var prop in propertiesToRemove)
        {
            product.Properties.Remove(prop);
        }

        await _repository.UpdateAsync(product);
        return Result.Success();
    }
    
    public async Task<Result<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdWithRelationsAsync(id, includeCategory: true, includeImages: true, includeManufacturer: true, includeProperties: true);
        if (product is null)
        {
            return Result.Failure<ProductDto>(Error.NotFound("Товар не найден"));
        }
        
        var category = product.ProductCategory;
        var parentCategory = category.ParentCategory;
        
        return Result.Success(new ProductDto(
            product.Id,
            product.Sku,
            product.Name, 
            product.Price, 
            product.StockQuantity, 
            product.CategoryId,
            product.ProductCategory.Name,
            product.ProductCategory.Description,
            parentCategory?.Id,
            parentCategory?.Name,
            parentCategory?.Description,
            product.ManufacturerId,
            product.Manufacturer.Name,
            product.Manufacturer.Country,
            product.Description, 
            product.IsAvailable,
            product.IsPrescriptionRequired,
            product.ExpirationDate,
            product.Images.Select(x => x.Url).ToList(),
            product.Properties.Select(x => new ProductPropertyDto(x.Key, x.Value)).ToList()
        ));
    }
    
    public async Task<Result<PaginatedList<ProductCardDto>>> GetPaginatedProductsAsync(ProductParameters query, int? userId = null)
    {
        var productsQuery = _repository.Query();

        if (query.CategoryIds is not null && query.CategoryIds.Any())
        {
            var categoryIds = new List<int>(query.CategoryIds);
            foreach (var categoryId in query.CategoryIds)
            {
                var childIds = await _productCategoryService.GetAllSubcategoryIdsAsync(categoryId);
                categoryIds.AddRange(childIds);
            }
            productsQuery = productsQuery.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (query.ManufacturerIds is not null && query.ManufacturerIds.Any())
        {
            productsQuery = productsQuery.Where(p => query.ManufacturerIds.Contains(p.ManufacturerId));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(query.Search.ToLower())); //EF.Functions.ILike(p.Name, $"%{query.Search}%")
        }

        if (query.PropertyFilters is not null && query.PropertyFilters.Any())
        {
            foreach (var filter in query.PropertyFilters)
            {
                var key = filter.Key;
                var values = filter.Value;

                if (values is not null && values.Any())
                {
                    productsQuery = productsQuery.Where(p => p.Properties.Any(prop => prop.Key == key && values.Contains(prop.Value)));
                }
            }
        }
        
        productsQuery = query.SortBy switch
        {
            "price" => query.SortOrder == "desc" ? productsQuery.OrderByDescending(p => p.Price) : productsQuery.OrderBy(p => p.Price),
            "datetime" => query.SortOrder == "desc" ? productsQuery.OrderByDescending(p => p.CreatedAt) : productsQuery.OrderBy(p => p.CreatedAt),
            _ => productsQuery.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await productsQuery.CountAsync();
        
        var pageItems = await productsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                ImageUrl = p.Images.OrderBy(x => x.Id).Select(x => x.Url).FirstOrDefault(),
                p.IsAvailable,
                p.IsPrescriptionRequired,
            })
            .AsNoTracking()
            .ToListAsync();

        var favoriteIds = userId is null ? new HashSet<int>() : (await _favoritesRepository.GetFavoriteProductIdsAsync(userId.Value)).ToHashSet();
        var cartItems = userId is null ? new Dictionary<int, int>() : (await _cartRepository.GetRawUserCartAsync(userId.Value)).ToDictionary(x => x.ProductId, x => x.Quantity);

        var items = pageItems.Select(p => new ProductCardDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.ImageUrl,
            p.IsAvailable,
            p.IsPrescriptionRequired,
            favoriteIds.Contains(p.Id),
            cartItems.TryGetValue(p.Id, out var qty) ? qty : 0 ) 
        ).ToList();


        return Result.Success(new PaginatedList<ProductCardDto>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<Result> DeleteAsync(int productId)
    {
        var product = await _repository.GetByIdWithRelationsAsync(productId);
        if (product is null)
        {
            return Result.Failure(Error.NotFound("Товар не найден"));
        }
        
        await _repository.DeleteAsync(product);
        return Result.Success();
    }
    
    public async Task<List<string>> GetSearchSuggestionsAsync(string query)
    {
        return await _repository.GetSearchSuggestionsAsync(query);
    }

    public async Task<Result<List<FilterOptionDto>>> GetFilterValuesAsync(int categoryId)
    {
        var categoryResult = await _productCategoryService.GetByIdAsync(categoryId);
        if (categoryResult.IsFailure)
        {
            return Result.Failure<List<FilterOptionDto>>(categoryResult.Error);
        }

        var category = categoryResult.Value;
        
        List<int> relevantCategoryIds;
        if (category.ParentCategoryId is null)
        {
            relevantCategoryIds = new List<int> { category.Id };
        }
        else
        {
            relevantCategoryIds = new List<int> { category.Id, category.ParentCategoryId.Value };
        }

        var products = await _repository.Query()
            .Where(p => relevantCategoryIds.Contains(p.CategoryId))
            .Select(p => p.Properties)
            .ToListAsync();

        var result = new Dictionary<string, HashSet<string>>();

        foreach (var propertyList in products)
        {
            foreach (var prop in propertyList)
            {
                if (!result.ContainsKey(prop.Key))
                    result[prop.Key] = new HashSet<string>();
                result[prop.Key].Add(prop.Value);
            }
        }

        var fields = await _productCategoryService.GetAllFieldsIncludingParentAsync(categoryId);

        var finalResult = result.Select(r =>
        {
            var field = fields.Value.FirstOrDefault(f => f.Key == r.Key);
            var label = field?.Label ?? r.Key;
            return new FilterOptionDto(
                r.Key,
                label,
                r.Value.OrderBy(v => v).ToList()
            );
        }).ToList();

        return Result.Success(finalResult);
    }
    
    private List<string> ValidateProductProperties(List<ProductPropertyDto> properties, List<CategoryFieldDto> categoryFields)
    {
        var errors = new List<string>();
        var propertyKeys = new HashSet<string>();

        foreach (var property in properties)
        {
            if (!propertyKeys.Add(property.Key))
            {
                errors.Add($"Дублирование ключа свойства: \"{property.Key}\"");
            }
        }

        foreach (var field in categoryFields.Where(x => x.IsRequired))
        {
            if (!properties.Any(x => x.Key == field.Key && !string.IsNullOrWhiteSpace(x.Value)))
            {
                errors.Add($"Обязательное поле \"{field.Key}\" не заполнено");
            }
        }

        var allowedFields = categoryFields.ToDictionary(x => x.Key, x => x.Type);
        foreach (var property in properties)
        {
            if (!allowedFields.TryGetValue(property.Key, out var expectedType))
            {
                errors.Add($"Недопустимое поле \"{property.Key}\"");
                continue;
            }

            if (!TypeValidationHelper.IsValidType(property.Value, expectedType))
            {
                errors.Add($"Неверный тип данных для поля \"{property.Key}\". Ожидался тип \"{expectedType}\".");
            }
        }

        return errors;
    }

    private async Task<string> GenerateNextSkuAsync()
    {
        var lastSku = await _repository.GetLastSkuAsync();
        var numberPart = lastSku.Split('-').LastOrDefault();
        if (int.TryParse(numberPart, out int lastNumber))
        {
            return $"PRD-{(lastNumber + 1):D6}";
        }
        return "PRD-000001";
    }
}