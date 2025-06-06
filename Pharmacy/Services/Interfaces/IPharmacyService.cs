using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IPharmacyService
{
    Task<Result<CreatedDto>> CreateAsync(CreatePharmacyDto dto);
    Task<Result<int?>> GetExistingPharmacyIdAsync(string name, string? osmId, double latitude, double longitude);
    Task<Result<int>> GetOrCreatePharmacyIdAsync(CreatePharmacyDto dto);
}