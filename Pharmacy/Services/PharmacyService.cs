using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class PharmacyService : IPharmacyService
{
    private readonly IPharmacyRepository _pharmacyRepository;
    private readonly IAddressRepository _addressRepository;

    public PharmacyService(IPharmacyRepository pharmacyRepository, IAddressRepository addressRepository)
    {
        _pharmacyRepository = pharmacyRepository;
        _addressRepository = addressRepository;
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
}