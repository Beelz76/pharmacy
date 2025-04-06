using System.Net;
using Pharmacy.Data.Repositories.Interfaces;
using Pharmacy.Endpoints.Manufacturer;
using Pharmacy.Models.Dtos;
using Pharmacy.Models.Entities;
using Pharmacy.Models.Enums;
using Pharmacy.Models.Result;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Services;

public class ManufacturerService : IManufacturerService
{
    private readonly IManufacturerRepository _repository;

    public ManufacturerService(IManufacturerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ManufacturerDto>>> GetAllAsync()
    {
        var manufacturers = (await _repository.GetAllAsync()).ToList();
        if (!manufacturers.Any())
        {
            return Result<IEnumerable<ManufacturerDto>>.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.Failure, "Производители не найдены");
        }

        return Result<IEnumerable<ManufacturerDto>>.Success(new List<ManufacturerDto>(manufacturers.Select(m => new ManufacturerDto(m.Name, m.Country))));
    }

    public async Task<Result<ManufacturerDto>> GetByIdAsync(int id)
    {
        var manufacturer = await _repository.GetByIdAsync(id);
        if (manufacturer is null)
        {
            return Result<ManufacturerDto>.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, "Производитель не найден");
        }

        return Result<ManufacturerDto>.Success(new ManufacturerDto(manufacturer.Name, manufacturer.Country));
    }

    public async Task<Result> CreateAsync(CreateManufacturerRequest request)
    {
        var manufacturerExists = await _repository.ExistsByNameAsync(request.Name);
        if (manufacturerExists)
        {
            return Result.Failure(HttpStatusCode.Conflict, ErrorTypeEnum.Conflict, "Производитель с таким названием уже существует");
        }

        var manufacturer = new Manufacturer
        {
            Name = request.Name,
            Country = request.Country
        };

        await _repository.AddAsync(manufacturer);
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(int id, UpdateManufacturerRequest request)
    {
        return await _repository.ExecuteInTransactionAsync(async () =>
        {
            var manufacturer = await _repository.GetByIdAsync(id);
            if (manufacturer is null)
            {
                return Result.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, "Производитель не найден");
            }

            if (manufacturer.Name != request.Name)
            {
                var manufacturerExists = await _repository.ExistsByNameAsync(request.Name, id);
                if (manufacturerExists)
                {
                    return Result.Failure(HttpStatusCode.Conflict, ErrorTypeEnum.Conflict, "Производитель с таким названием уже существует");
                }
            }

            manufacturer.Name = request.Name;
            manufacturer.Country = request.Country;

            await _repository.UpdateAsync(manufacturer);
            return Result.Success();
        });
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var manufacturer = await _repository.GetByIdAsync(id);
        if (manufacturer is null)
        {
            return Result.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, "Производитель не найден");
        }

        await _repository.DeleteAsync(manufacturer);
        return Result.Success();
    }
}