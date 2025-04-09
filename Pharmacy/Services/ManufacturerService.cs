using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Endpoints.Manufacturers;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

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
            return Result.Failure<IEnumerable<ManufacturerDto>>(Error.NotFound("Производители не найдены"));
        }

        var manufacturersDto = new List<ManufacturerDto>(manufacturers.Select(m => new ManufacturerDto(m.Id, m.Name, m.Country)));
        
        return Result.Success<IEnumerable<ManufacturerDto>>(manufacturersDto);
    }

    public async Task<Result<ManufacturerDto>> GetByIdAsync(int id)
    {
        var manufacturer = await _repository.GetByIdAsync(id);
        if (manufacturer is null)
        {
            return Result.Failure<ManufacturerDto>(Error.NotFound("Производитель не найден"));
        }

        return Result.Success(new ManufacturerDto(manufacturer.Id, manufacturer.Name, manufacturer.Country));
    }

    public async Task<Result> CreateAsync(CreateManufacturerRequest request)
    {
        var existingManufacturer = await _repository.GetByNameAsync(request.Name);
        if (existingManufacturer is not null)
        {
            return Result.Failure(Error.Conflict("Производитель с таким названием уже существует"));
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
                return Result.Failure(Error.NotFound("Производитель не найден"));
            }

            if (manufacturer.Name != request.Name)
            {
                var existingManufacturer = await _repository.GetByNameAsync(request.Name, excludeId: id);
                if (existingManufacturer is not null)
                {
                    return Result.Failure(Error.Conflict("Производитель с таким названием уже существует"));
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
            return Result.Failure(Error.NotFound("Производитель не найден"));
        }

        await _repository.DeleteAsync(manufacturer);
        return Result.Success();
    }
}