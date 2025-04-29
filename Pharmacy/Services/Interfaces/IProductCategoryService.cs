using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductCategoryService
{
    Task<Result<IEnumerable<ProductCategoryDto>>> GetAllAsync();
    Task<Result<CreatedDto>> CreateAsync(string name, string description, List<CategoryFieldDto> fields);
    Task<Result> UpdateBasicInfoAsync(int id, string name, string description);
    Task<Result> AddFieldsAsync(int categoryId, List<CategoryFieldDto> fields);
    Task<Result> DeleteFieldsAsync(int categoryId, List<int> fieldIds);
    Task<Result> UpdateFieldsAsync(int categoryId, List<CategoryFieldDto> fields);
    Task<Result> DeleteAsync(int id);
    Task<bool> ExistsAsync(int categoryId);
    Task<Result<IEnumerable<CategoryFieldDto>>> GetCategoryFieldsAsync(int categoryId);
}