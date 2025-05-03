using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Helpers;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _repository;
    private readonly IProductRepository _productRepository;
    
    public ProductCategoryService(IProductCategoryRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    // public async Task<Result<IEnumerable<ProductCategoryDto>>> GetAllAsync()
    // {
    //     var categories = (await _repository.GetAllAsync()).ToList();
    //     if (!categories.Any())
    //     {
    //         return Result.Failure<IEnumerable<ProductCategoryDto>>(Error.NotFound("Категории не найдены"));
    //     }
    //
    //     var categoriesDto = new List<ProductCategoryDto>(categories.Select(x => new ProductCategoryDto(x.Id, x.Name, x.Description)));
    //     
    //     return Result.Success<IEnumerable<ProductCategoryDto>>(categoriesDto);
    // }
    
    public async Task<Result<IEnumerable<ProductCategoryWithSubDto>>> GetAllWithSubcategoriesAsync()
    {
        var categories = (await _repository.GetAllAsync()).ToList();

        if (!categories.Any())
        {
            return Result.Failure<IEnumerable<ProductCategoryWithSubDto>>(Error.NotFound("Категории не найдены"));
        }
        
        var result = categories
            .Where(c => c.ParentCategoryId == null)
            .Select(c => new ProductCategoryWithSubDto(
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId,
                c.Subcategories.Select(sc => new ProductCategoryDto(sc.Id, sc.Name, sc.Description)).ToList()
            )).ToList();
        
        return Result.Success<IEnumerable<ProductCategoryWithSubDto>>(result);
    }

    public async Task<Result<ProductCategoryWithSubDto>> GetWithSubcategoriesByIdAsync(int categoryId)
    {
        var category = await _repository.GetByIdWithRelationsAsync(categoryId, includeSubcategories: true);
        if (category is null)
        {
            return Result.Failure<ProductCategoryWithSubDto>(Error.NotFound("Категория не найдена"));
        }

        var dto = new ProductCategoryWithSubDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.Subcategories.Select(sc => new ProductCategoryDto(sc.Id, sc.Name, sc.Description)).ToList()
        );

        return Result.Success(dto);
    }
    
    public async Task<Result<IEnumerable<ProductCategoryDto>>> GetSubcategoriesAsync(int categoryId)
    {
        var subcategories = (await _repository.GetSubcategoriesAsync(categoryId)).ToList();

        if (!subcategories.Any())
        {
            return Result.Failure<IEnumerable<ProductCategoryDto>>(Error.NotFound("Подкатегории не найдены"));
        }

        return Result.Success(subcategories.Select(x => new ProductCategoryDto(x.Id, x.Name, x.Description)));
    }
    
    public async Task<Result<CreatedDto>> CreateAsync(string name, string description, int? parentCategoryId, List<CategoryFieldDto> fields)
    {
        if (await _repository.ExistsAsync(name: name, description: description))
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Категория с таким именем или описанием уже существует"));
        }

        if (parentCategoryId.HasValue && !await _repository.ExistsAsync(parentCategoryId.Value))
        {
            return Result.Failure<CreatedDto>(Error.NotFound("Родительская категория не найдена"));
        }
        
        var duplicateKeys = fields
            .GroupBy(x => x.Key.ToLower())
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();
        
        if (duplicateKeys.Any())
        {
            return Result.Failure<CreatedDto>(Error.Failure($"Обнаружены дублирующиеся ключи: {string.Join(", ", duplicateKeys)}"));
        }

        if (parentCategoryId.HasValue)
        {
            var allFieldsResult = await GetAllFieldsIncludingParentAsync(parentCategoryId.Value);
            var conflicts = fields.Where(x => allFieldsResult.Value.Any(f => f.Key.Equals(x.Key, StringComparison.OrdinalIgnoreCase))).ToList();
            if (conflicts.Any())
            {
                return Result.Failure<CreatedDto>(Error.Conflict($"Поля с такими ключами уже существуют: {string.Join(", ", conflicts.Select(c => c.Key))}"));
            }
        }
        
        var category = new ProductCategory
        {
            Name = name, 
            Description = description,
            ParentCategoryId = parentCategoryId,
            Fields = fields.Select(x => new ProductCategoryField
            {
                FieldKey = x.Key,
                FieldLabel = x.Label,
                FieldType = x.Type,
                IsRequired = x.IsRequired,
                IsFilterable = x.IsFilterable
            }).ToList()
        };
        await _repository.AddAsync(category);
        return Result.Success(new CreatedDto(category.Id));
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, string name, string description, int? parentCategoryId)
    {
        var category = await _repository.GetByIdWithRelationsAsync(id);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        if (parentCategoryId.HasValue && !await _repository.ExistsAsync(parentCategoryId.Value))
        {
            return Result.Failure(Error.NotFound("Родительская категория не найдена"));
        }

        if (parentCategoryId == id)
        {
            return Result.Failure(Error.Failure("Категория не может быть родителем самой себя"));
        }
        
        if (await _repository.ExistsAsync(name: name, description: description, excludeId: id))
        {
            return Result.Failure(Error.Conflict("Категория с таким именем или описанием уже существует"));
        }

        category.Name = name;
        category.Description = description;
        category.ParentCategoryId = parentCategoryId;
        await _repository.UpdateAsync(category);
        return Result.Success();
    }

    public async Task<Result> AddFieldsAsync(int categoryId, List<CategoryFieldDto> fields)
    {
        var category = await _repository.GetByIdWithRelationsAsync(categoryId, includeFields:true);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }
        
        var duplicateKeys = fields
            .GroupBy(x => x.Key.ToLower())
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Any())
        {
            return Result.Failure(Error.Failure($"Обнаружены дублирующиеся ключи в запросе: {string.Join(", ", duplicateKeys)}"));
        }

        var allFieldsResult = await GetAllFieldsIncludingParentAsync(categoryId);
        var existingKeys = allFieldsResult.Value.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var conflicts = fields
            .Where(x => existingKeys.Contains(x.Key))
            .Select(x => x.Key)
            .ToList();
        
        if (conflicts.Any())
        {
            return Result.Failure(Error.Conflict($"Поля с такими ключами уже существуют в родительской или текущей категории: {string.Join(", ", conflicts)}"));
        }

        foreach (var field in fields)
        {
            category.Fields.Add(new ProductCategoryField
            {
                FieldKey = field.Key,
                FieldLabel = field.Label,
                FieldType = field.Type,
                IsRequired = false,
                IsFilterable = field.IsFilterable
            });
        }

        await _repository.UpdateAsync(category);
        return Result.Success();
    }
    
    public async Task<Result> DeleteFieldsAsync(int categoryId, List<int> fieldIds)
    {
        var category = await _repository.GetByIdWithRelationsAsync(categoryId, includeFields: true);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }
        
        var existingIds = category.Fields.Select(f => f.Id).ToHashSet();
        var invalidIds = fieldIds.Except(existingIds).ToList();
        if (invalidIds.Any())
        {
            return Result.Failure(Error.Failure($"Следующие поля не принадлежат категории: {string.Join(", ", invalidIds)}"));

        }

        var errors = new List<string>();
        var toRemove = category.Fields.Where(f => fieldIds.Contains(f.Id)).ToList();

        foreach (var field in toRemove)
        {
            if (field.IsRequired && await _productRepository.HasFieldUsedAsync(categoryId, field.FieldKey))
            {
                errors.Add($"Нельзя удалить обязательное поле \"{field.FieldLabel}\", пока оно используется хотя бы одним товаром.");
            }
        }

        if (errors.Any())
        {
            return Result.Failure(Error.Failure("Ошибки при удалении полей", errors));
        }

        foreach (var field in toRemove)
        {
            category.Fields.Remove(field);
        }

        await _repository.UpdateAsync(category);
        return Result.Success();
    }
    
    public async Task<Result> UpdateFieldsAsync(int categoryId, List<CategoryFieldDto> fields)
    {
        var category = await _repository.GetByIdWithRelationsAsync(categoryId, includeFields: true);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        var errors = new List<string>();
        
        var duplicateKeys = fields
            .GroupBy(x => x.Key.ToLower())
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Any())
        {
            errors.Add($"Обнаружены дублирующиеся ключи: {string.Join(", ", duplicateKeys)}");
        }

        var allFieldsResult = await GetAllFieldsIncludingParentAsync(categoryId);
        
        foreach (var fieldDto in fields)
        {
            if (fieldDto.Id is null)
            {
                errors.Add("При обновлении полей ID обязателен для всех полей");
                continue;
            }

            var currentField = category.Fields.FirstOrDefault(f => f.Id == fieldDto.Id);
            if (currentField is null)
            {
                errors.Add($"Поле с ID {fieldDto.Id.Value} не найдено в текущей категории");
                continue;
            }

            if (allFieldsResult.Value.Any(f => f.Key.Equals(fieldDto.Key, StringComparison.OrdinalIgnoreCase) && f.Id != fieldDto.Id))
            {
                errors.Add($"Поле с ключом '{fieldDto.Key}' уже существует");
                continue;
            }

            if (!currentField.IsRequired && fieldDto.IsRequired && await _productRepository.HasMissingFieldAsync(categoryId, currentField.FieldKey))
            {
                errors.Add($"Нельзя сделать поле \"{currentField.FieldLabel}\" обязательным, пока есть товары без заполненного значения.");
                continue;
            }

            if (currentField.FieldType != fieldDto.Type && await HasInvalidFieldValuesAsync(categoryId, currentField.FieldKey, fieldDto.Type))
            {
                errors.Add($"Нельзя изменить тип поля \"{currentField.FieldLabel}\", так как значения товаров не соответствуют новому типу.");
                continue;
            }
            
            currentField.FieldKey = fieldDto.Key;
            currentField.FieldLabel = fieldDto.Label;
            currentField.FieldType = fieldDto.Type;
            currentField.IsRequired = fieldDto.IsRequired;
            currentField.IsFilterable = fieldDto.IsFilterable;
        }

        if (errors.Any())
        {
            return Result.Failure(Error.Failure("Обнаружены ошибки", errors));
        }
        
        await _repository.UpdateAsync(category);
        return await ValidateProductsInCategoryAsync(categoryId);
    }
    
    public async Task<Result> DeleteAsync(int id)
    {
        var category = await _repository.GetByIdWithRelationsAsync(id, includeSubcategories: true);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        if (category.Subcategories.Any())
        {
            return Result.Failure(Error.Conflict("Нельзя удалить категорию, у которой есть подкатегории"));
        }
        
        if (await _productRepository.ExistsAsync(categoryId: id))
        {
            return Result.Failure(Error.Conflict("Нельзя удалить категорию, пока к ней привязаны товары"));
        }
        
        await _repository.DeleteAsync(category);
        return Result.Success();
    }

    public async Task<bool> ExistsAsync(int categoryId)
    {
        return await _repository.ExistsAsync(categoryId);
    }
    
    public async Task<Result<ICollection<CategoryFieldDto>>> GetAllFieldsIncludingParentAsync(int? categoryId, bool checkForExistence = false)
    {
        if (checkForExistence)
        {
            var categoryExists = await _repository.ExistsAsync(categoryId!.Value);
            if (!categoryExists)
            {
                return Result.Failure<ICollection<CategoryFieldDto>>(Error.NotFound("Категория не найдена"));
            }
        }
        
        var result = new List<CategoryFieldDto>();
        while (categoryId.HasValue)
        {
            var category = await _repository.GetByIdWithRelationsAsync(categoryId.Value, includeFields: true, includeParent: true);
            if (category == null) break;
            result.AddRange(category.Fields.Select(x => new CategoryFieldDto(
                x.Id,
                x.FieldKey,
                x.FieldLabel,
                x.FieldType,
                x.IsRequired,
                x.IsFilterable)));
            categoryId = category.ParentCategory?.Id;
        }
        return result;
    }
    
    public async Task<List<int>> GetAllSubcategoryIdsAsync(int categoryId)
    {
        return await _repository.GetChildrenIdsAsync(categoryId);
    }
    
    private async Task<Result> ValidateProductsInCategoryAsync(int categoryId)
    {
        var categoryFieldsResult = await GetAllFieldsIncludingParentAsync(categoryId);
        if (categoryFieldsResult.IsFailure)
        {
            return Result.Failure(categoryFieldsResult.Error);
        }

        var allowedFields = categoryFieldsResult.Value.ToDictionary(x => x.Key, x => x);

        var products = await _productRepository.QueryWithProperties()
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();

        var invalidProducts = new List<(int ProductId, List<string> Errors)>();

        foreach (var product in products)
        {
            var errors = new List<string>();
            var propDict = product.Properties.ToDictionary(x => x.Key, x => x.Value);

            foreach (var field in allowedFields.Values)
            {
                if (field.IsRequired && (!propDict.ContainsKey(field.Key) || string.IsNullOrWhiteSpace(propDict[field.Key])))
                {
                    errors.Add($"Обязательное поле \"{field.Label}\" не заполнено в товаре ID: {product.Id}");
                }
            }

            foreach (var prop in product.Properties)
            {
                if (!allowedFields.ContainsKey(prop.Key))
                {
                    errors.Add($"Недопустимое поле \"{prop.Key}\" в товаре ID: {product.Id}");
                }
            }

            if (errors.Any())
            {
                invalidProducts.Add((product.Id, errors));
            }
        }

        if (invalidProducts.Any())
        {
            var allErrors = invalidProducts.SelectMany(x => x.Errors).ToList();
            return Result.Failure(Error.Failure("Обнаружены ошибки", allErrors));
        }

        return Result.Success();
    }
    
    private async Task<bool> HasInvalidFieldValuesAsync(int categoryId, string fieldKey, string newType)
    {
        var products = await _productRepository.QueryWithProperties()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

        foreach (var product in products)
        {
            var value = product.Properties.FirstOrDefault(p => p.Key == fieldKey)?.Value;
            if (!string.IsNullOrWhiteSpace(value) && !TypeValidationHelper.IsValidType(value, newType))
            {
                return true;
            }
        }

        return false;
    }
}