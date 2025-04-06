using Pharmacy.Endpoints.Manufacturer;
using Pharmacy.Models.Dtos;
using Pharmacy.Models.Result;

namespace Pharmacy.Services.Interfaces;

public interface IManufacturerService
{
    Task<Result<IEnumerable<ManufacturerDto>>> GetAllAsync();
    Task<Result<ManufacturerDto>> GetByIdAsync(int id);
    Task<Result> CreateAsync(CreateManufacturerRequest request);
    Task<Result> UpdateAsync(int Id, UpdateManufacturerRequest manufacturer);
    Task<Result> DeleteAsync(int id);
}