using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Category;
using Pharmacy.Shared.Dto.Product;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductCategoryService
{
    //Task<Result<IEnumerable<ProductCategoryDto>>> GetAllAsync();
    Task<Result<IEnumerable<ProductCategoryWithSubDto>>> GetAllWithSubcategoriesAsync();
    Task<Result<ProductCategoryWithSubDto>> GetByIdAsync(int categoryId, bool includeSubcategories = false);
    Task<Result<IEnumerable<ProductCategoryDto>>> GetSubcategoriesAsync(int categoryId);
    Task<Result<CreatedDto>> CreateAsync(string name, string description, int? parentCategoryId, List<CategoryFieldDto> fields);
    Task<Result> UpdateBasicInfoAsync(int id, string name, string description, int? parentCategoryId);
    Task<Result> AddFieldsAsync(int categoryId, List<CategoryFieldDto> fields);
    Task<Result> DeleteFieldsAsync(int categoryId, List<int> fieldIds);
    Task<Result> UpdateFieldsAsync(int categoryId, List<CategoryFieldDto> fields);
    Task<Result> DeleteAsync(int id);
    Task<bool> ExistsAsync(int categoryId);
    Task<Result<ICollection<CategoryFieldDto>>> GetAllFieldsIncludingParentAsync(int? categoryId, bool checkForExistence = false);
    Task<List<int>> GetAllSubcategoryIdsAsync(int categoryId);
}