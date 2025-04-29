using Pharmacy.Endpoints.Products;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IProductService
{
    Task<Result<CreatedDto>> CreateProductAsync(CreateProductRequest request);
    Task<Result> UpdateAsync(int id, UpdateProductRequest request);
    Task<Result<ProductDto>> GetByIdAsync(int id);
    Task<Result<PaginatedList<ProductDto>>> GetPaginatedProductsAsync(ProductParameters query);
    Task<Result> DeleteAsync(int productId);
    Task<List<string>> GetSearchSuggestionsAsync(string query);
}