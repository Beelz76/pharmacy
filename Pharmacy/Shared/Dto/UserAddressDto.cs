namespace Pharmacy.Shared.Dto;

public record UserAddressDto(
    int Id,
    AddressDto Address,
    string? Apartment,
    string? Entrance,
    string? Floor,
    string? Comment,
    string FullAddress
);