using Microsoft.Extensions.Caching.Hybrid;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Endpoints.Manufacturers;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Manufacturer;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class ManufacturerService : IManufacturerService
{
    private readonly IManufacturerRepository _repository;
    private readonly HybridCache _cache;

    public ManufacturerService(IManufacturerRepository repository, HybridCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<IEnumerable<ManufacturerDto>>> GetAllAsync()
    {
        var manufacturersDto = await _cache.GetOrCreateAsync(
            "manufacturers-all",
            async ct =>
            {
                var manufacturers = (await _repository.GetAllAsync()).ToList();
                if (!manufacturers.Any()) return null;

                return new List<ManufacturerDto>(manufacturers.Select(m => new ManufacturerDto(m.Id, m.Name, m.Country)));
            });
        
        if (manufacturersDto is null)
        {
            return Result.Failure<IEnumerable<ManufacturerDto>>(Error.NotFound("Производители не найдены"));
        }
        
        return Result.Success<IEnumerable<ManufacturerDto>>(manufacturersDto);
    }

    public async Task<Result<ManufacturerDto>> GetByIdAsync(int id)
    {
        var dto = await _cache.GetOrCreateAsync(
            $"manufacturer-{id}",
            async ct =>
            {
                var manufacturer = await _repository.GetByIdAsync(id);
                return manufacturer is null ? null : new ManufacturerDto(manufacturer.Id, manufacturer.Name, manufacturer.Country);
            });

        if (dto is null)
        {
            return Result.Failure<ManufacturerDto>(Error.NotFound("Производитель не найден"));
        }
        
        return Result.Success(dto);
    }
    
    public async Task<bool> ExistsAsync(int manufacturerId)
    {
        return await _repository.ExistsAsync(manufacturerId);
    }

    public async Task<Result<CreatedDto>> CreateAsync(CreateManufacturerRequest request)
    {
        if (await _repository.ExistsAsync(name: request.Name))
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Производитель с таким названием уже существует"));
        }

        var manufacturer = new Manufacturer
        {
            Name = request.Name,
            Country = request.Country
        };

        await _repository.AddAsync(manufacturer);
        await _cache.RemoveAsync("manufacturers-all");
        await _cache.RemoveAsync("manufacturer-countries");
        return Result.Success(new CreatedDto(manufacturer.Id));
    }

    public async Task<Result> UpdateAsync(int id, UpdateManufacturerRequest request)
    {
        var manufacturer = await _repository.GetByIdAsync(id);
        if (manufacturer is null) 
        {
            return Result.Failure(Error.NotFound("Производитель не найден"));
        }

        if (await _repository.ExistsAsync(name: request.Name, excludeId: id)) 
        { 
            return Result.Failure(Error.Conflict("Производитель с таким названием уже существует"));
        }
        
        manufacturer.Name = request.Name;
        manufacturer.Country = request.Country;
        await _repository.UpdateAsync(manufacturer);
        await _cache.RemoveAsync("manufacturers-all");
        await _cache.RemoveAsync("manufacturer-countries");
        await _cache.RemoveAsync($"manufacturer-{id}");
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var manufacturer = await _repository.GetByIdAsync(id);
        if (manufacturer is null)
        {
            return Result.Failure(Error.NotFound("Производитель не найден"));
        }

        await _repository.DeleteAsync(manufacturer);
        await _cache.RemoveAsync("manufacturers-all");
        await _cache.RemoveAsync("manufacturer-countries");
        await _cache.RemoveAsync($"manufacturer-{id}");
        return Result.Success();
    }
    
    public async Task<Result<List<string>>> GetCountriesAsync()
    {
        var countries = await _cache.GetOrCreateAsync(
            "manufacturer-countries",
            async ct =>
            {
                var all = (await _repository.GetAllAsync()).ToList();
                if (!all.Any()) return null;
                return all.Select(m => m.Country).Distinct().OrderBy(c => c).ToList();
            });

        if (countries is null)
        {
            return Result.Failure<List<string>>(Error.NotFound("Страны не найдены"));
        }

        return Result.Success(countries);
    }
}