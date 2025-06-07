using Pharmacy.Database.Entities;
using Pharmacy.Endpoints.PharmacyProducts;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IPharmacyProductService
{
    Task<Result<IEnumerable<PharmacyProductDto>>> GetByPharmacyAsync(int pharmacyId);
    Task<PharmacyProductDto?> GetAsync(int pharmacyId, int productId);
    Task<Result<CreatedDto>> AddAsync(int pharmacyId, AddPharmacyProductRequest request);
    Task<Result> UpdateAsync(int pharmacyId, int productId, UpdatePharmacyProductRequest request);
    Task<Result> DeleteAsync(int pharmacyId, int productId);
    Task<Result<List<PharmacyProductDto>>> ValidateOrAddProductsAsync(int pharmacyId, List<(int productId, int quantity)> items);
    Task<Result> UpdateStockQuantityAsync(int pharmacyId, int productId, int stockQuantity);
}