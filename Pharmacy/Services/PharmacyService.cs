using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Address;
using Pharmacy.Shared.Dto.Pharmacy;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class PharmacyService : IPharmacyService
{
    private readonly IPharmacyRepository _pharmacyRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly HybridCache _cache;

    public PharmacyService(IPharmacyRepository pharmacyRepository, IAddressRepository addressRepository, HybridCache cache)
    {
        _pharmacyRepository = pharmacyRepository;
        _addressRepository = addressRepository;
        _cache = cache;
    }

    public async Task<Result<IEnumerable<PharmacyDto>>> GetAllAsync()
    {
        var dto = await _cache.GetOrCreateAsync(
            "pharmacies-all",
            async ct =>
            {
                var pharmacies = (await _pharmacyRepository.GetAllAsync()).ToList();
                if (!pharmacies.Any()) return null;

                return pharmacies.Select(p => new PharmacyDto(
                    p.Id,
                    p.Name,
                    p.Phone,
                    new AddressDto(
                        p.Address.Id,
                        p.Address.OsmId,
                        p.Address.Region,
                        p.Address.State,
                        p.Address.City,
                        p.Address.Suburb,
                        p.Address.Street,
                        p.Address.HouseNumber,
                        p.Address.Postcode,
                        p.Address.Latitude,
                        p.Address.Longitude
                    ))).ToList();
            });

        if (dto is null)
        {
            return Result.Failure<IEnumerable<PharmacyDto>>(Error.NotFound("Аптеки не найдены"));
        }
        
        return Result.Success<IEnumerable<PharmacyDto>>(dto);
    }

    public async Task<Result<PharmacyDto>> GetByIdAsync(int id)
    {
        var dto = await _cache.GetOrCreateAsync(
            $"pharmacy-{id}",
            async ct =>
            {
                var pharmacy = await _pharmacyRepository.GetByIdAsync(id);
                if (pharmacy is null) return null;

                return new PharmacyDto(
                    pharmacy.Id,
                    pharmacy.Name,
                    pharmacy.Phone,
                    new AddressDto(
                        pharmacy.Address.Id,
                        pharmacy.Address.OsmId,
                        pharmacy.Address.Region,
                        pharmacy.Address.State,
                        pharmacy.Address.City,
                        pharmacy.Address.Suburb,
                        pharmacy.Address.Street,
                        pharmacy.Address.HouseNumber,
                        pharmacy.Address.Postcode,
                        pharmacy.Address.Latitude,
                        pharmacy.Address.Longitude
                    ));
            });

        if (dto is null)
        {
            return Result.Failure<PharmacyDto>(Error.NotFound("Аптека не найдена"));
        }

        return Result.Success(dto);
    }
    
    public async Task<Result<PaginatedList<PharmacyDto>>> GetPaginatedAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _pharmacyRepository.Query();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        var totalCount = await query.CountAsync();

        var pharmacies = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dto = pharmacies.Select(p => new PharmacyDto(
            p.Id,
            p.Name,
            p.Phone,
            new AddressDto(
                p.Address.Id,
                p.Address.OsmId,
                p.Address.Region,
                p.Address.State,
                p.Address.City,
                p.Address.Suburb,
                p.Address.Street,
                p.Address.HouseNumber,
                p.Address.Postcode,
                p.Address.Latitude,
                p.Address.Longitude
            ))).ToList();

        return Result.Success(new PaginatedList<PharmacyDto>(dto, totalCount, pageNumber, pageSize));
    }
    
    public async Task<Result<CreatedDto>> CreateAsync(CreatePharmacyDto dto)
    {
        if (await _pharmacyRepository.ExistsAsync(dto.Name, dto.Address.OsmId, dto.Address.Latitude, dto.Address.Longitude))
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Аптека с таким адресом уже существует"));
        }
        
        var address = await _addressRepository.GetOrCreateAddressAsync(new Address
        {
            OsmId = dto.Address.OsmId,
            Region = dto.Address.Region,
            State = dto.Address.State,
            City = dto.Address.City,
            Suburb = dto.Address.Suburb,
            Street = dto.Address.Street,
            HouseNumber = dto.Address.HouseNumber,
            Postcode = dto.Address.Postcode,
            Latitude = dto.Address.Latitude,
            Longitude = dto.Address.Longitude
        });

        var pharmacy = new Database.Entities.Pharmacy
        {
            Name = dto.Name,
            Phone = dto.Phone,
            AddressId = address.Id,
            IsActive = true
        };

        await _pharmacyRepository.AddAsync(pharmacy);
        return Result.Success(new CreatedDto(pharmacy.Id));
    }
    
    public async Task<Result<int?>> GetExistingPharmacyIdAsync(string name, string? osmId, double latitude, double longitude)
    {
        var pharmacy = await _pharmacyRepository.GetByOsmAndCoordinatesAsync(name, osmId, latitude, longitude);
        return Result.Success(pharmacy?.Id);
    }
    
    public async Task<Result<int>> GetOrCreatePharmacyIdAsync(CreatePharmacyDto dto)
    {
        var existingIdResult = await GetExistingPharmacyIdAsync(dto.Name, dto.Address.OsmId, dto.Address.Latitude, dto.Address.Longitude);
        if (existingIdResult.Value.HasValue)
            return Result.Success(existingIdResult.Value.Value);

        var createResult = await CreateAsync(dto);
        if (createResult.IsFailure)
            return Result.Failure<int>(createResult.Error);

        return Result.Success(createResult.Value.Id);
    }
    
    public async Task<Result<int?>> GetNearestPharmacyIdAsync(double latitude, double longitude)
    {
        var nearest = await _pharmacyRepository.GetNearestAsync(latitude, longitude);
        return Result.Success(nearest?.Id);
    }
}