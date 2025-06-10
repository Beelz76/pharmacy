using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Products;
using Pharmacy.ExternalServices;
using Pharmacy.Helpers;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Category;
using Pharmacy.Shared.Dto.Manufacturer;
using Pharmacy.Shared.Dto.Product;
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
    private readonly IStorageProvider _storage;
    private readonly HybridCache _cache;
    
    public ProductService(IDateTimeProvider dateTimeProvider, IProductRepository repository, IProductCategoryService productCategoryService, IManufacturerService manufacturerService, ICartRepository cartRepository, IFavoritesRepository favoritesRepository, IStorageProvider storage, HybridCache cache)
    {
        _dateTimeProvider = dateTimeProvider;
        _repository = repository;
        _productCategoryService = productCategoryService;
        _manufacturerService = manufacturerService;
        _cartRepository = cartRepository;
        _favoritesRepository = favoritesRepository;
        _storage = storage;
        _cache = cache;
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
            CategoryId = request.CategoryId,
            ManufacturerId = request.ManufacturerId,
            Description = request.Description,
            ExtendedDescription = request.ExtendedDescription,
            IsGloballyDisabled = !request.IsAvailable,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            Properties = request.Properties.Select(p => new ProductProperty
            {
                Key = p.Key,
                Value = p.Value
            }).ToList()
        };

        await _repository.AddAsync(product);
        await _cache.RemoveByTagAsync(new[] { "products", $"filter-values-{product.CategoryId}" });
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
        product.CategoryId = request.CategoryId;
        product.ManufacturerId = request.ManufacturerId;
        product.Description = request.Description;
        product.IsGloballyDisabled = !request.IsAvailable;
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
        var allowedFields = categoryFields.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);
        
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
        await _cache.RemoveByTagAsync(new[] { "products", $"filter-values-{product.CategoryId}" });
        await _cache.RemoveAsync($"product-{id}");
        return Result.Success();
    }
    
    public async Task<Result<ProductDto>> GetByIdAsync(int id)
    {
        var dto = await _cache.GetOrCreateAsync(
            $"product-{id}",
            async ct =>
            {
                var product = await _repository.GetByIdWithRelationsAsync(id, includeCategory: true, includeImages: true, includeManufacturer: true, includeProperties: true);
                if (product is null)
                {
                    return null;
                }

                var category = product.ProductCategory;
                var parentCategory = category.ParentCategory;

                var fieldLabels = category.Fields
                    .ToDictionary(x => x.FieldKey, x => x.FieldLabel);

                var propertyDtos = product.Properties.Select(prop =>
                {
                    var label = fieldLabels.TryGetValue(prop.Key, out var l) ? l : prop.Key;
                    return new ProductPropertyDto(prop.Key, label, prop.Value);
                }).ToList();

                return new ProductDto(
                    product.Id,
                    product.Sku,
                    product.Name,
                    product.Price,
                    new ProductCategoryDto(product.CategoryId, product.ProductCategory.Name, product.ProductCategory.Description),
                    new ProductCategoryNullableDto(parentCategory?.Id, parentCategory?.Name, parentCategory?.Description),
                    new ManufacturerDto(product.ManufacturerId, product.Manufacturer.Name, product.Manufacturer.Country),
                    product.Description,
                    product.ExtendedDescription,
                    !product.IsGloballyDisabled,
                    product.Images.Select(x => new ProductImageDto(x.Id, x.Url.StartsWith("http") ? x.Url : _storage.GetPublicUrl(x.Url))).ToList(),
                    propertyDtos
                );
            },
            tags: new[] { "products" });

        if (dto is null)
        {
            return Result.Failure<ProductDto>(Error.NotFound("Товар не найден"));
        }

        return Result.Success(dto);
    }
    
    public async Task<Result<PaginatedList<ProductCardDto>>> GetPaginatedProductsAsync(ProductParameters query, int? userId = null)
    {
        var cacheKey = BuildProductsCacheKey(userId, query);
        
        var data = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
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

                if (query.Countries is not null && query.Countries.Any())
                {
                    productsQuery = productsQuery.Where(p => query.Countries.Contains(p.Manufacturer.Country));
                }
                
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    productsQuery =
                        productsQuery.Where(p =>
                            p.Name.ToLower()
                                .Contains(query.Search.ToLower())); //EF.Functions.ILike(p.Name, $"%{query.Search}%")
                }

                if (query.IsAvailable is not null)
                {
                    productsQuery = productsQuery.Where(x => x.IsGloballyDisabled != query.IsAvailable);
                }

                if (query.PropertyFilters is not null && query.PropertyFilters.Any())
                {
                    foreach (var filter in query.PropertyFilters)
                    {
                        var key = filter.Key;
                        var values = filter.Value;

                        if (values is not null && values.Any())
                        {
                            productsQuery = productsQuery.Where(p =>
                                p.Properties.Any(prop => prop.Key == key && values.Contains(prop.Value)));
                        }
                    }
                }

                productsQuery = query.SortBy switch
                {
                    "price" => query.SortOrder == "desc"
                        ? productsQuery.OrderByDescending(p => p.Price)
                        : productsQuery.OrderBy(p => p.Price),
                    "datetime" => query.SortOrder == "desc"
                        ? productsQuery.OrderByDescending(p => p.CreatedAt)
                        : productsQuery.OrderBy(p => p.CreatedAt),
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
                        p.CategoryId,
                        CategoryName = p.ProductCategory.Name,
                        ImageUrl = p.Images.OrderBy(x => x.Id).Select(x => x.Url).FirstOrDefault(),
                        IsAvailable = !p.IsGloballyDisabled,
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: ct);

                var favoriteIds = userId is null ? new HashSet<int>() : (await _favoritesRepository.GetFavoriteProductIdsAsync(userId.Value)).ToHashSet();
                var cartItems = userId is null ? new Dictionary<int, int>() : (await _cartRepository.GetRawUserCartAsync(userId.Value)).ToDictionary(x => x.ProductId, x => x.Quantity);

                var items = pageItems.Select(p => new ProductCardDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    !string.IsNullOrWhiteSpace(p.ImageUrl) && p.ImageUrl.StartsWith("http") ? p.ImageUrl : _storage.GetPublicUrl(p.ImageUrl),
                    p.IsAvailable,
                    favoriteIds.Contains(p.Id),
                    cartItems.TryGetValue(p.Id, out var qty) ? qty : 0,
                    p.CategoryId,
                    p.CategoryName)
                ).ToList();
                
                return new PaginatedList<ProductCardDto>(items, totalCount, query.PageNumber, query.PageSize);
            },
            tags: new[] { "products" });
        
        return Result.Success(data);
    }

    public async Task<Result> DeleteAsync(int productId)
    {
        var product = await _repository.GetByIdWithRelationsAsync(productId);
        if (product is null)
        {
            return Result.Failure(Error.NotFound("Товар не найден"));
        }
        
        await _repository.DeleteAsync(product);
        await _cache.RemoveByTagAsync(new[] { "products", $"filter-values-{product.CategoryId}" });
        await _cache.RemoveAsync($"product-{productId}");
        return Result.Success();
    }
    
    public async Task<List<string>> GetSearchSuggestionsAsync(string query)
    {
        return await _repository.GetSearchSuggestionsAsync(query);
    }

    public async Task<Result<List<FilterOptionDto>>> GetFilterValuesAsync(int categoryId)
    {
        var cacheKey = $"filter-values-{categoryId}";

        var data = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var categoryResult = await _productCategoryService.GetByIdAsync(categoryId);
                if (categoryResult.IsFailure)
                    return null;

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
                    .ToListAsync(cancellationToken: ct);

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
                    var field = fields.Value.FirstOrDefault(x => x.Key == r.Key);
                    var label = field?.Label ?? r.Key;
                    return new FilterOptionDto(
                        r.Key,
                        label,
                        r.Value.OrderBy(v => v).ToList()
                    );
                }).ToList();

                return finalResult;
            },
            tags: new[] { $"filter-values-{categoryId}" });


        if (data is null)
        {
            return Result.Failure<List<FilterOptionDto>>(Error.NotFound("Категория не найдена"));
        }

        return Result.Success(data);
    }
    
    public async Task<Result<ProductDto>> GetBySkuAsync(string sku)
    {
        var product = await _repository.GetBySkuAsync(sku);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Товар не найден"));

        var fieldLabels = product.ProductCategory.Fields
            .ToDictionary(x => x.FieldKey, x => x.FieldLabel);

        var propertyDtos = product.Properties.Select(prop =>
        {
            var label = fieldLabels.TryGetValue(prop.Key, out var l) ? l : prop.Key;
            return new ProductPropertyDto(prop.Key, label, prop.Value);
        }).ToList();

        var parentCategory = product.ProductCategory.ParentCategory;

        var dto = new ProductDto(
            product.Id,
            product.Sku,
            product.Name,
            product.Price,
            new ProductCategoryDto(product.CategoryId, product.ProductCategory.Name, product.ProductCategory.Description),
            new ProductCategoryNullableDto(parentCategory?.Id, parentCategory?.Name, parentCategory?.Description),
            new ManufacturerDto(product.ManufacturerId, product.Manufacturer.Name, product.Manufacturer.Country),
            product.Description,
            product.ExtendedDescription,
            !product.IsGloballyDisabled,
            product.Images.Select(x => new ProductImageDto(x.Id, x.Url.StartsWith("http") ? x.Url :_storage.GetPublicUrl(x.Url))).ToList(),
            propertyDtos
        );

        return Result.Success(dto);
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
    
    private static string BuildProductsCacheKey(int? userId, ProductParameters q)
    {
        var parts = new List<string>
        {
            "products",
            userId?.ToString() ?? "none",
            q.PageNumber.ToString(),
            q.PageSize.ToString()
        };

        if (!string.IsNullOrWhiteSpace(q.Search))
            parts.Add(q.Search);
        if (q.CategoryIds is { Count: > 0 })
            parts.Add(string.Join(',', q.CategoryIds));
        if (q.ManufacturerIds is { Count: > 0 })
            parts.Add(string.Join(',', q.ManufacturerIds));
        if (q.Countries is { Count: > 0 })
            parts.Add(string.Join(',', q.Countries));
        if (!string.IsNullOrWhiteSpace(q.SortBy))
            parts.Add(q.SortBy);
        if (!string.IsNullOrWhiteSpace(q.SortOrder))
            parts.Add(q.SortOrder);
        if (q.IsAvailable is not null)
            parts.Add(q.IsAvailable.ToString());

        return string.Join('-', parts);
    }
}