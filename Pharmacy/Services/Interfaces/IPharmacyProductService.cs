using Pharmacy.Endpoints.PharmacyProducts;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IPharmacyProductService
{
    Task<Result<IEnumerable<PharmacyProductDto>>> GetByPharmacyAsync(int pharmacyId);
    Task<Result<CreatedDto>> AddAsync(int pharmacyId, AddPharmacyProductRequest request);
    Task<Result> UpdateAsync(int pharmacyId, int productId, UpdatePharmacyProductRequest request);
    Task<Result> DeleteAsync(int pharmacyId, int productId);
}