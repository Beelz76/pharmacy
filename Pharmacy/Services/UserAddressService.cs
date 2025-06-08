using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Endpoints.UserAddresses;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Address;
using Pharmacy.Shared.Dto.User;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class UserAddressService : IUserAddressService
{
    private readonly IUserAddressRepository _repository;
    private readonly IAddressRepository _addressRepository;

    public UserAddressService(IUserAddressRepository repository, IAddressRepository addressRepository)
    {
        _repository = repository;
        _addressRepository = addressRepository;
    }

    public async Task<Result<IEnumerable<UserAddressDto>>> GetAllAsync(int userId)
    {
        var addresses = await _repository.GetByUserIdAsync(userId);
        var result = addresses.Select(ToDto);
        return Result.Success(result);
    }

    public async Task<Result<UserAddressDto>> GetByIdAsync(int userId, int userAddressId)
    {
        var address = await _repository.GetByIdAsync(userId, userAddressId);
        if (address == null)
        {
            return Result.Failure<UserAddressDto>(Error.NotFound("Адрес не найден"));
        }

        return Result.Success(ToDto(address));
    }

    public async Task<Result<CreatedDto>> CreateAsync(int userId, CreateUserAddressRequest request)
    {
        var address = await _addressRepository.GetOrCreateAddressAsync(new Address
        {
            OsmId = request.Address.OsmId,
            Region = request.Address.Region,
            State = request.Address.State,
            City = request.Address.City,
            Suburb = request.Address.Suburb,
            Street = request.Address.Street,
            HouseNumber = request.Address.HouseNumber,
            Postcode = request.Address.Postcode,
            Latitude = request.Address.Latitude,
            Longitude = request.Address.Longitude
        });
        
        var existingAddresses = await _repository.GetByUserIdAsync(userId);
        var alreadyExists = existingAddresses.Any(x => IsSameUserAddress(x, address, request));

        if (alreadyExists)
        {
            return Result.Failure<CreatedDto>(Error.Conflict("Такой адрес уже добавлен"));
        }

        var userAddress = new UserAddress
        {
            UserId = userId,
            AddressId = address.Id,
            Apartment = request.Apartment,
            Entrance = request.Entrance,
            Floor = request.Floor,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(userAddress);
        return Result.Success(new CreatedDto(userAddress.Id));
    }


    public async Task<Result> UpdateAsync(int userId, int userAddressId, UpdateUserAddressRequest request)
    {
        var userAddress = await _repository.GetByIdAsync(userId, userAddressId);
        if (userAddress == null)
        {
            return Result.Failure(Error.NotFound("Адрес не найден"));
        }

        var address = await _addressRepository.GetOrCreateAddressAsync(new Address
        {
            OsmId = request.Address.OsmId,
            Region = request.Address.Region,
            State = request.Address.State,
            City = request.Address.City,
            Suburb = request.Address.Suburb,
            Street = request.Address.Street,
            HouseNumber = request.Address.HouseNumber,
            Postcode = request.Address.Postcode,
            Latitude = request.Address.Latitude,
            Longitude = request.Address.Longitude
        });
        
        var existingAddresses = await _repository.GetByUserIdAsync(userId);
        var alreadyExists = existingAddresses
            .Where(x => x.Id != userAddressId)
            .Any(x => IsSameUserAddress(x, address, request));

        if (alreadyExists)
        {
            return Result.Failure(Error.Conflict("Такой адрес уже добавлен"));
        }

        userAddress.AddressId = address.Id;
        userAddress.Apartment = request.Apartment;
        userAddress.Entrance = request.Entrance;
        userAddress.Floor = request.Floor;
        userAddress.Comment = request.Comment;
        userAddress.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(userAddress);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int userId, int userAddressId)
    {
        var userAddress = await _repository.GetByIdAsync(userId, userAddressId);
        if (userAddress == null)
        {
            return Result.Failure(Error.NotFound("Адрес не найден"));
        }

        await _repository.DeleteAsync(userAddress);
        return Result.Success();
    }

    private static UserAddressDto ToDto(UserAddress ua) =>
        new(
            ua.Id,
            new AddressDto(
                ua.Address.Id,
                ua.Address.OsmId,
                ua.Address.Region,
                ua.Address.State,
                ua.Address.City,
                ua.Address.Suburb,
                ua.Address.Street,
                ua.Address.HouseNumber,
                ua.Address.Postcode,
                ua.Address.Latitude,
                ua.Address.Longitude
            ),
            ua.Apartment,
            ua.Entrance,
            ua.Floor,
            ua.Comment,
            AddressExtensions.FormatAddress(ua)
        );
    
    private bool IsSameUserAddress(UserAddress a, Address bAddress, CreateUserAddressRequest b)
    {
        return a.AddressId == bAddress.Id &&
               (a.Apartment ?? "") == (b.Apartment ?? "") &&
               (a.Entrance ?? "") == (b.Entrance ?? "") &&
               (a.Floor ?? "") == (b.Floor ?? "") &&
               (a.Comment ?? "") == (b.Comment ?? "");
    }

    private bool IsSameUserAddress(UserAddress a, Address bAddress, UpdateUserAddressRequest b)
    {
        return a.AddressId == bAddress.Id &&
               (a.Apartment ?? "") == (b.Apartment ?? "") &&
               (a.Entrance ?? "") == (b.Entrance ?? "") &&
               (a.Floor ?? "") == (b.Floor ?? "") &&
               (a.Comment ?? "") == (b.Comment ?? "");
    }
}