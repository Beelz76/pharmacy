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
    private readonly IManufacturerService _manufacturerService;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public ProductService(IDateTimeProvider dateTimeProvider, IProductRepository repository, IProductCategoryService productCategoryService, IManufacturerService manufacturerService)
    {
        _dateTimeProvider = dateTimeProvider;
        _repository = repository;
        _productCategoryService = productCategoryService;
        _manufacturerService = manufacturerService;
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
        
        var product = new Product
        {
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
        var product = await _repository.GetByIdAsync(id);
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
        var product = await _repository.GetDetailsByIdAsync(id);
        if (product is null)
        {
            return Result.Failure<ProductDto>(Error.NotFound("Товар не найден"));
        }
        
        var category = product.ProductCategory;
        var parentCategory = category.ParentCategory;
        
        return Result.Success(new ProductDto(
            product.Id, 
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
    
    public async Task<Result<PaginatedList<ProductDto>>> GetPaginatedProductsAsync(ProductParameters query)
    {
        var productsQuery = _repository.QueryWithProperties();

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

        var rawProducts = await productsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(p => p.ProductCategory)
                .ThenInclude(c => c.ParentCategory)
            .Include(p => p.Manufacturer)
            .Include(p => p.Images)
            .ToListAsync();
        
        var items = rawProducts.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Price,
            p.StockQuantity,
            p.CategoryId,
            p.ProductCategory.Name,
            p.ProductCategory.Description,
            p.ProductCategory.ParentCategory?.Id,
            p.ProductCategory.ParentCategory?.Name,
            p.ProductCategory.ParentCategory?.Description,
            p.ManufacturerId,
            p.Manufacturer.Name,
            p.Manufacturer.Country,
            p.Description,
            p.IsAvailable,
            p.IsPrescriptionRequired,
            p.ExpirationDate,
            p.Images.Select(x => x.Url).ToList(),
            p.Properties.Select(x => new ProductPropertyDto(x.Key, x.Value)).ToList()
        )).ToList();

        return Result.Success(new PaginatedList<ProductDto>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<Result> DeleteAsync(int productId)
    {
        var product = await _repository.GetByIdAsync(productId);
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

}