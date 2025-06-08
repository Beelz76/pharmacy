using Pharmacy.Endpoints.Products;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Product;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductService
{
    Task<Result<CreatedDto>> CreateProductAsync(CreateProductRequest request);
    Task<Result> UpdateAsync(int id, UpdateProductRequest request);
    Task<Result<ProductDto>> GetByIdAsync(int id);
    Task<Result<PaginatedList<ProductCardDto>>> GetPaginatedProductsAsync(ProductParameters query, int? userId = null);
    Task<Result> DeleteAsync(int productId);
    Task<Result<List<FilterOptionDto>>> GetFilterValuesAsync(int categoryId);
    Task<List<string>> GetSearchSuggestionsAsync(string query);
}