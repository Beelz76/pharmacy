using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Pharmacy;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IPharmacyService
{
    Task<Result<CreatedDto>> CreateAsync(CreatePharmacyDto dto);
    Task<Result<IEnumerable<PharmacyDto>>> GetAllAsync();
    Task<Result<PharmacyDto>> GetByIdAsync(int id);
    Task<Result<int?>> GetExistingPharmacyIdAsync(string name, string? osmId, double latitude, double longitude);
    Task<Result<int>> GetOrCreatePharmacyIdAsync(CreatePharmacyDto dto);
    Task<Result<int?>> GetNearestPharmacyIdAsync(double latitude, double longitude);
    Task<Result<PaginatedList<PharmacyDto>>> GetPaginatedAsync(string? search, int pageNumber, int pageSize);
    Task<Result> UpdateAsync(int id, UpdatePharmacyDto dto);
    Task<Result> DeleteAsync(int id);
}