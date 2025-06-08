using Pharmacy.Shared.Dto.Address;

namespace Pharmacy.Shared.Dto.User;

public record UserAddressDto(
    int Id,
    AddressDto Address,
    string? Apartment,
    string? Entrance,
    string? Floor,
    string? Comment,
    string FullAddress
);