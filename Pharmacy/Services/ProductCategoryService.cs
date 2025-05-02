using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
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

    public async Task<Result<IEnumerable<ProductCategoryDto>>> GetAllAsync()
    {
        var categories = (await _repository.GetAllAsync()).ToList();
        if (!categories.Any())
        {
            return Result.Failure<IEnumerable<ProductCategoryDto>>(Error.NotFound("Категории не найдены"));
        }

        var categoriesDto = new List<ProductCategoryDto>(categories.Select(x => new ProductCategoryDto(x.Id, x.Name, x.Description)));
        
        return Result.Success<IEnumerable<ProductCategoryDto>>(categoriesDto);
    }
    
    public async Task<Result<CreatedDto>> CreateAsync(string name, string description, List<CategoryFieldDto> fields)
    {
        if (await _repository.ExistsByNameOrDescriptionAsync(name, description))
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Категория с таким именем или описанием уже существует"));
        }

        var duplicateKeys = fields
            .GroupBy(x => x.Key)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();
        
        if (duplicateKeys.Any())
        {
            return Result.Failure<CreatedDto>(Error.Failure($"Обнаружены дублирующиеся ключи: {string.Join(", ", duplicateKeys)}"));
        }
        
        var category = new ProductCategory
        {
            Name = name, 
            Description = description,
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

    public async Task<Result> UpdateBasicInfoAsync(int id, string name, string description)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        if (await _repository.ExistsByNameOrDescriptionAsync(name, description))
        {
            return Result.Failure(Error.Conflict("Категория с таким именем или описанием уже существует"));
        }

        category.Name = name;
        category.Description = description;
        await _repository.UpdateAsync(category);
        return Result.Success();
    }

    public async Task<Result> AddFieldsAsync(int categoryId, List<CategoryFieldDto> fields)
    {
        var category = await _repository.GetByIdAsync(categoryId);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }
        
        var duplicateKeys = fields
            .GroupBy(x => x.Key)
            .Where(x => x.Count() > 1).
            Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Any())
        {
            return Result.Failure(Error.Failure($"Обнаружены дублирующиеся ключи в запросе: {string.Join(", ", duplicateKeys)}"));
        }

        var existingKeys = category.Fields.Select(f => f.FieldKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var conflicts = fields
            .Where(x => existingKeys
            .Contains(x.Key))
            .Select(x => x.Key)
            .ToList();
        
        if (conflicts.Any())
        {
            return Result.Failure(Error.Conflict($"Поля с такими ключами уже существуют: {string.Join(", ", conflicts)}"));
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
        var category = await _repository.GetByIdAsync(categoryId);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        var fieldsToRemove = category.Fields
            .Where(f => fieldIds.Contains(f.Id))
            .ToList();
        
        if (!fieldsToRemove.Any())
        {
            return Result.Failure(Error.NotFound("Не найдены поля для удаления"));
        }
        
        var errors = new List<string>();

        foreach (var field in fieldsToRemove)
        {
            if (field.IsRequired)
            {
                var isUsed = await _productRepository.HasFieldUsedAsync(categoryId, field.FieldKey);
                if (isUsed)
                {
                    errors.Add($"Нельзя удалить обязательное поле \"{field.FieldLabel}\", пока оно используется хотя бы одним товаром.");
                }
            }
        }
        
        if (errors.Any())
        {
            return Result.Failure(Error.Failure("Ошибка при удалении полей", errors));
        }

        foreach (var field in fieldsToRemove)
        {
            category.Fields.Remove(field);
        }

        await _repository.UpdateAsync(category);
        return Result.Success();
    }
    
    public async Task<Result> UpdateFieldsAsync(int categoryId, List<CategoryFieldDto> fields)
    {
        var category = await _repository.GetByIdAsync(categoryId);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        var duplicateKeys = fields
            .GroupBy(x => x.Key)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Any())
        {
            return Result.Failure(Error.Failure($"Обнаружены дублирующиеся ключи: {string.Join(", ", duplicateKeys)}"));
        }

        var errors = new List<string>();
        foreach (var fieldDto in fields)
        {
            if (fieldDto.Id is null)
            {
                errors.Add("При обновлении полей ID обязателен для всех полей");
                continue;
            }

            var existingField = category.Fields.FirstOrDefault(f => f.Id == fieldDto.Id.Value);
            if (existingField is null)
            {
                errors.Add($"Поле с ID {fieldDto.Id.Value} не найдено");
                continue;
            }
            
            if (!existingField.IsRequired && fieldDto.IsRequired)
            {
                var hasMissingInProducts = await _productRepository.HasMissingFieldAsync(categoryId, existingField.FieldKey);

                if (hasMissingInProducts)
                {
                    errors.Add($"Нельзя сделать поле \"{existingField.FieldLabel}\" обязательным, пока есть товары без заполненного значения.");
                    continue;
                }
            }
            
            if (existingField.FieldType != fieldDto.Type)
            {
                var valuesInvalid = await HasInvalidFieldValuesAsync(categoryId, existingField.FieldKey, fieldDto.Type);
                if (valuesInvalid)
                {
                    errors.Add($"Нельзя изменить тип поля \"{existingField.FieldLabel}\", так как существующие товары содержат значения, не соответствующие новому типу");
                    continue;
                }
            }
            
            existingField.FieldKey = fieldDto.Key;
            existingField.FieldLabel = fieldDto.Label;
            existingField.FieldType = fieldDto.Type;
            existingField.IsRequired = fieldDto.IsRequired;
            existingField.IsFilterable = fieldDto.IsFilterable;
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
        var category = await _repository.GetByIdAsync(id);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Категория не найдена"));
        }

        if (await _productRepository.ExistsByCategoryAsync(id))
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

    public async Task<Result<IEnumerable<CategoryFieldDto>>> GetCategoryFieldsAsync(int categoryId)
    {
        var category = await _repository.GetByIdAsync(categoryId);
        if (category is null)
        {
            return Result.Failure<IEnumerable<CategoryFieldDto>>(Error.NotFound("Категория не найдена"));
        }
        
        var fields = await _repository.GetCategoryFieldsAsync(categoryId);
        
        return Result.Success(fields.Select(x => new CategoryFieldDto(
            x.Id,
            x.FieldKey,
            x.FieldLabel,
            x.FieldType,
            x.IsRequired,
            x.IsRequired
        )));
    }
    
    private async Task<Result> ValidateProductsInCategoryAsync(int categoryId)
    {
        var categoryFieldsResult = await GetCategoryFieldsAsync(categoryId);
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
            if (!string.IsNullOrWhiteSpace(value) && !IsValidType(value, newType))
            {
                return true;
            }
        }

        return false;
    }
    
    private bool IsValidType(string value, string expectedType)
    {
        return expectedType.ToLower() switch
        {
            "string" => !string.IsNullOrWhiteSpace(value),
            "number" => decimal.TryParse(value, out _),
            "integer" => int.TryParse(value, out _),
            "boolean" => bool.TryParse(value, out _),
            "date" => DateTime.TryParse(value, out _),
            _ => true
        };
    }
}